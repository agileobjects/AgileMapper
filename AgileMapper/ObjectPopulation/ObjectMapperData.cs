namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Extensions;
    using Members;
    using Members.Sources;

    internal class ObjectMapperData : BasicMapperData, IMemberMapperData
    {
        private readonly List<ObjectMapperData> _childMapperDatas;
        private readonly MethodInfo _mapChildMethod;
        private readonly MethodInfo _mapElementMethod;
        private readonly MethodInfo _mapRecursionMethod;
        private readonly Dictionary<string, DataSourceSet> _dataSourcesByTargetMemberName;
        private ObjectMapperData _entryPointMapperData;

        private ObjectMapperData(
            IObjectMappingData mappingData,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            int? dataSourceIndex,
            ObjectMapperData parent,
            bool isForStandaloneMapping,
            bool isPartOfDerivedTypeMapping)
            : base(
                  mappingData.MappingContext.RuleSet,
                  sourceMember.Type,
                  targetMember.Type,
                  targetMember,
                  parent)
        {
            MapperContext = mappingData.MappingContext.MapperContext;
            _childMapperDatas = new List<ObjectMapperData>();
            DataSourceIndex = dataSourceIndex.GetValueOrDefault();

            MappingDataObject = GetMappingDataObject(parent);
            SourceMember = sourceMember;
            ParentObject = Expression.Property(MappingDataObject, "Parent");
            SourceObject = Expression.Property(MappingDataObject, "Source");
            TargetObject = Expression.Property(MappingDataObject, "Target");
            CreatedObject = Expression.Property(MappingDataObject, "CreatedObject");
            EnumerableIndex = Expression.Property(MappingDataObject, "EnumerableIndex");
            NestedAccessFinder = new NestedAccessFinder(MappingDataObject);

            _mapChildMethod = GetMapMethod(MappingDataObject.Type, 4);
            _mapElementMethod = GetMapMethod(MappingDataObject.Type, 3);
            _mapRecursionMethod = MappingDataObject.Type.GetMethod("MapRecursion");

            _dataSourcesByTargetMemberName = new Dictionary<string, DataSourceSet>();

            if (targetMember.IsEnumerable)
            {
                RequiresElementMapping = !targetMember.ElementType.IsSimple();
                EnumerablePopulationBuilder = new EnumerablePopulationBuilder(this);
                InstanceVariable = EnumerablePopulationBuilder.TargetVariable;
            }
            else
            {
                TargetTypeHasNotYetBeenMapped = IsTargetTypeFirstMapping(parent);
                TargetTypeWillNotBeMappedAgain = IsTargetTypeLastMapping();

                InstanceVariable = Expression
                    .Variable(TargetType, TargetType.GetVariableNameInCamelCase());
            }

            ReturnLabelTarget = Expression.Label(TargetType, "Return");
            IsForStandaloneMapping = isForStandaloneMapping;

            if (IsForStandaloneMapping)
            {
                RequiredMapperFuncsByKey = new Dictionary<ObjectMapperKeyBase, LambdaExpression>();
            }

            if (IsRoot)
            {
                IsPartOfDerivedTypeMapping = isPartOfDerivedTypeMapping;
                return;
            }

            IsPartOfDerivedTypeMapping = isPartOfDerivedTypeMapping || parent.IsPartOfDerivedTypeMapping;
            parent._childMapperDatas.Add(this);
            Parent = parent;
        }

        #region Setup

        private ParameterExpression GetMappingDataObject(ObjectMapperData parent)
        {
            var mdType = typeof(ObjectMappingData<,>).MakeGenericType(SourceType, TargetType);

            var variableNameIndex = default(int?);

            while (parent != null)
            {
                if (parent.MappingDataObject.Type == mdType)
                {
                    variableNameIndex = variableNameIndex.HasValue ? (variableNameIndex + 1) : 2;
                }

                parent = parent.Parent;
            }

            var mappingDataVariableName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}To{1}Data{2}",
                SourceType.GetShortVariableName(),
                TargetType.GetShortVariableName().ToPascalCase(),
                variableNameIndex);

            var parameter = Parameters.Create(mdType, mappingDataVariableName);

            return parameter;
        }

        private bool IsTargetTypeFirstMapping(ObjectMapperData parent)
        {
            if (IsRoot)
            {
                return true;
            }

            while (parent != null)
            {
                if (parent.HasTypeBeenMapped(TargetType, this))
                {
                    return false;
                }

                parent = parent.Parent;
            }

            return true;
        }

        private bool HasTypeBeenMapped(Type targetType, IBasicMapperData requestingMapperData)
        {
            var mappedType = TargetMember.IsEnumerable ? TargetMember.ElementType : TargetType;

            if (mappedType.IsAssignableFrom(targetType))
            {
                return true;
            }

            foreach (var childMapperData in ChildMapperDatas)
            {
                if (childMapperData == requestingMapperData)
                {
                    break;
                }

                if (childMapperData.HasTypeBeenMapped(targetType, this))
                {
                    return true;
                }
            }

            if ((Parent != null) && (requestingMapperData != Parent))
            {
                return Parent.HasTypeBeenMapped(targetType, this);
            }

            return false;
        }

        private bool IsTargetTypeLastMapping()
        {
            return !TypeHasACompatibleChildMember(TargetType, TargetType, new List<Type>());
        }

        private static bool TypeHasACompatibleChildMember(
            Type targetType,
            Type parentType,
            ICollection<Type> checkedTypes)
        {
            if (checkedTypes.Contains(parentType))
            {
                return true;
            }

            checkedTypes.Add(parentType);

            var childTargetMembers = GlobalContext.Instance.MemberFinder.GetWriteableMembers(parentType);

            foreach (var childMember in childTargetMembers)
            {
                if (childMember.IsSimple)
                {
                    continue;
                }

                if (childMember.IsComplex)
                {
                    if (childMember.Type.IsAssignableFrom(targetType))
                    {
                        return true;
                    }

                    return TypeHasACompatibleChildMember(targetType, childMember.Type, checkedTypes);
                }

                if (childMember.ElementType.IsComplex() && childMember.ElementType.IsAssignableFrom(targetType))
                {
                    return true;
                }

                return TypeHasACompatibleChildMember(targetType, childMember.ElementType, checkedTypes);
            }

            return false;
        }

        private static MethodInfo GetMapMethod(Type mappingDataType, int numberOfArguments)
        {
            return mappingDataType
                .GetPublicInstanceMethods()
                .First(m => (m.Name == "Map") && (m.GetParameters().Length == numberOfArguments));
        }

        #endregion

        #region Factory Method

        public static ObjectMapperData For<TSource, TTarget>(IObjectMappingData mappingData)
        {
            var membersSource = mappingData.MapperKey.GetMembersSource(mappingData.Parent);
            var sourceMember = membersSource.GetSourceMember<TSource>();
            var targetMember = membersSource.GetTargetMember<TTarget>();
            int? dataSourceIndex;
            ObjectMapperData parentMapperData;
            bool isForStandaloneMapping;

            if (mappingData.IsRoot)
            {
                if (mappingData.IsPartOfDerivedTypeMapping)
                {
                    sourceMember = sourceMember.WithType(typeof(TSource));
                    targetMember = targetMember.WithType(typeof(TTarget));
                }

                dataSourceIndex = null;
                parentMapperData = null;
                isForStandaloneMapping = true;
            }
            else
            {
                sourceMember = sourceMember.WithType(typeof(TSource));
                targetMember = targetMember.WithType(typeof(TTarget));
                dataSourceIndex = (membersSource as IChildMembersSource)?.DataSourceIndex;
                parentMapperData = mappingData.Parent.MapperData;
                isForStandaloneMapping = mappingData.MapperKey.MappingTypes.RuntimeTypesNeeded;
            }

            return new ObjectMapperData(
                mappingData,
                sourceMember,
                targetMember,
                dataSourceIndex,
                parentMapperData,
                isForStandaloneMapping,
                mappingData.IsPartOfDerivedTypeMapping);
        }

        #endregion

        public MapperContext MapperContext { get; }

        public ObjectMapperData Parent { get; }

        public IEnumerable<ObjectMapperData> ChildMapperDatas => _childMapperDatas;

        public bool IsForStandaloneMapping { get; }

        public bool IsPartOfDerivedTypeMapping { get; }

        public int DataSourceIndex { get; set; }

        public bool RequiresChildMapping => _dataSourcesByTargetMemberName.Any();

        public bool RequiresElementMapping { get; }

        public IQualifiedMember GetSourceMemberFor(string targetMemberRegistrationName, int dataSourceIndex)
            => _dataSourcesByTargetMemberName[targetMemberRegistrationName][dataSourceIndex].SourceMember;

        public QualifiedMember GetTargetMemberFor(string targetMemberRegistrationName)
            => TargetMember.GetChildMember(targetMemberRegistrationName);

        public ParameterExpression MappingDataObject { get; }

        public Expression ParentObject { get; }

        public IQualifiedMember SourceMember { get; }

        public bool TargetTypeHasNotYetBeenMapped { get; }

        public bool TargetTypeWillNotBeMappedAgain { get; }

        public Expression SourceObject { get; }

        public Expression TargetObject { get; }

        public Expression EnumerableIndex { get; }

        public ParameterExpression InstanceVariable { get; }

        public NestedAccessFinder NestedAccessFinder { get; }

        public EnumerablePopulationBuilder EnumerablePopulationBuilder { get; }

        public Expression CreatedObject { get; }

        public LabelTarget ReturnLabelTarget { get; }

        public ObjectMapperData EntryPointMapperData
            => _entryPointMapperData ?? (_entryPointMapperData = GetNearestEntryPointObjectMapperData());

        private ObjectMapperData GetNearestEntryPointObjectMapperData()
        {
            var mapperData = this;

            while (!mapperData.IsEntryPoint())
            {
                mapperData = mapperData.Parent;
            }

            return mapperData;
        }

        private bool IsEntryPoint()
        {
            if (IsRoot || IsForStandaloneMapping)
            {
                return true;
            }

            return TargetMember.IsRecursionRoot();
        }

        public void RegisterRequiredMapperFunc(IObjectMappingData mappingData)
        {
            var nearestStandaloneMapperData = GetNearestStandaloneMapperData();

            if (nearestStandaloneMapperData.RequiredMapperFuncsByKey.ContainsKey(mappingData.MapperKey))
            {
                return;
            }

            nearestStandaloneMapperData.RequiredMapperFuncsByKey.Add(mappingData.MapperKey, null);

            var mappingLambda = mappingData.Mapper.MappingLambda;
            nearestStandaloneMapperData.RequiredMapperFuncsByKey[mappingData.MapperKey] = mappingLambda;
        }

        private ObjectMapperData GetNearestStandaloneMapperData()
        {
            var mapperData = this;

            while (!mapperData.IsForStandaloneMapping)
            {
                mapperData = mapperData.Parent;
            }

            return mapperData;
        }

        public bool HasMapperFuncs => (RequiredMapperFuncsByKey != null) && RequiredMapperFuncsByKey.Any();

        public Dictionary<ObjectMapperKeyBase, LambdaExpression> RequiredMapperFuncsByKey { get; }

        public MethodCallExpression GetMapCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            return GetMapChildCall(
                MappingDataObject,
                _mapChildMethod,
                sourceObject,
                targetMember,
                dataSourceIndex);
        }

        private MethodCallExpression GetMapChildCall(
            Expression subject,
            MethodInfo method,
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            var mapCall = Expression.Call(
                subject,
                method.MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(InstanceVariable),
                Expression.Constant(targetMember.RegistrationName),
                Expression.Constant(dataSourceIndex));

            return mapCall;
        }

        public MethodCallExpression GetMapCall(Expression sourceElement, Expression targetElement)
        {
            var mapCall = Expression.Call(
                MappingDataObject,
                _mapElementMethod.MakeGenericMethod(sourceElement.Type, targetElement.Type),
                sourceElement,
                targetElement,
                EnumerablePopulationBuilder.Counter);

            return mapCall;
        }

        public MethodCallExpression GetMapRecursionCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            return GetMapChildCall(
                EntryPointMapperData.MappingDataObject,
                EntryPointMapperData._mapRecursionMethod,
                sourceObject,
                targetMember,
                dataSourceIndex);
        }

        public void RegisterTargetMemberDataSourcesIfRequired(QualifiedMember targetMember, DataSourceSet dataSources)
        {
            if (targetMember.IsSimple)
            {
                return;
            }

            _dataSourcesByTargetMemberName.Add(targetMember.RegistrationName, dataSources);
        }
    }
}