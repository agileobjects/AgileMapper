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
        private readonly MethodInfo _mapObjectMethod;
        private readonly MethodInfo _mapEnumerableElementMethod;
        private readonly Dictionary<string, DataSourceSet> _dataSourcesByTargetMemberName;
        private ParameterExpression _mapperFuncVariable;

        private ObjectMapperData(
            IObjectMappingData mappingData,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            int? dataSourceIndex,
            ObjectMapperData parent,
            bool isForStandaloneMapping,
            bool isPartOfDerivedTypeMapping)
            : base(
                  mappingData.RuleSet,
                  sourceMember.Type,
                  targetMember.Type,
                  targetMember,
                  parent)
        {
            MapperContext = mappingData.MappingContext.MapperContext;
            _childMapperDatas = new List<ObjectMapperData>();
            DataSourceIndex = dataSourceIndex.GetValueOrDefault();

            MappingDataObject = GetMappingDataObject();
            SourceMember = sourceMember;
            ParentObject = Expression.Property(MappingDataObject, "Parent");
            SourceObject = Expression.Property(MappingDataObject, "Source");
            TargetObject = Expression.Property(MappingDataObject, "Target");
            CreatedObject = Expression.Property(MappingDataObject, "CreatedObject");
            EnumerableIndex = Expression.Property(MappingDataObject, "EnumerableIndex");
            NestedAccessFinder = new NestedAccessFinder(MappingDataObject);

            _mapObjectMethod = GetMapMethod(MappingDataObject.Type, 4);
            _mapEnumerableElementMethod = GetMapMethod(MappingDataObject.Type, 3);

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
                RequiredMapperFuncsByVariable = new Dictionary<ParameterExpression, Expression>();
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

        private ParameterExpression GetMappingDataObject()
        {
            var mdType = typeof(ObjectMappingData<,>).MakeGenericType(SourceType, TargetType);

            var mappingDataVariableName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}To{1}Data",
                SourceType.GetShortVariableName(),
                TargetType.GetShortVariableName().ToPascalCase());

            var parameter = mdType.GetOrCreateParameter(mappingDataVariableName);

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

        private bool IsTargetTypeLastMapping() => !TypeHasACompatibleChildMember(TargetType, TargetType);

        private static bool TypeHasACompatibleChildMember(Type targetType, Type parentType)
        {
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

                    return TypeHasACompatibleChildMember(targetType, childMember.Type);
                }

                if (childMember.ElementType.IsComplex() && childMember.ElementType.IsAssignableFrom(targetType))
                {
                    return true;
                }

                return TypeHasACompatibleChildMember(targetType, childMember.ElementType);
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

        public static ObjectMapperData For<TSource, TTarget>(
            IMembersSource membersSource,
            IObjectMappingData mappingData)
        {
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

        public bool IsForInlineMapping => !IsForStandaloneMapping;

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

        public ParameterExpression FindRequiredMapperFuncVariable()
            => Parent.GetMapperFuncVariableFor(TargetMember);

        private ParameterExpression GetMapperFuncVariableFor(QualifiedMember targetMember)
        {
            if (targetMember.LeafMember != TargetMember.LeafMember)
            {
                return Parent.GetMapperFuncVariableFor(targetMember);
            }

            if (_mapperFuncVariable != null)
            {
                return _mapperFuncVariable;
            }

            var nearestStandaloneMapperData = GetNearestStandaloneMapperData();
            var mapperFuncType = typeof(MapperFunc<,>).MakeGenericType(SourceType, TargetType);

            _mapperFuncVariable = nearestStandaloneMapperData
                .RequiredMapperFuncsByVariable
                .FirstOrDefault(fbv => fbv.Key.Type == mapperFuncType)
                .Key;

            if (_mapperFuncVariable != null)
            {
                return _mapperFuncVariable;
            }

            var mapperFuncVariableName = string.Format(
                CultureInfo.InvariantCulture,
                "map{0}To{1}",
                SourceMember.Name.ToPascalCase(),
                TargetType.GetVariableNameInPascalCase());

            _mapperFuncVariable = Parameters.Create(mapperFuncType, mapperFuncVariableName);

            return _mapperFuncVariable;
        }

        public bool HasRequiredMapperFunc => _mapperFuncVariable != null;

        public void AddMapperFuncBody(Expression mappingLambda)
        {
            var nearestStandaloneMapperData = GetNearestStandaloneMapperData();

            nearestStandaloneMapperData.RequiredMapperFuncsByVariable[_mapperFuncVariable] = mappingLambda;
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

        public LabelTarget ReturnLabelTarget { get; }

        public bool HasMapperFuncs => (RequiredMapperFuncsByVariable != null) && RequiredMapperFuncsByVariable.Any();

        public Dictionary<ParameterExpression, Expression> RequiredMapperFuncsByVariable { get; }

        public MethodCallExpression GetMapCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            var mapCall = Expression.Call(
                MappingDataObject,
                _mapObjectMethod.MakeGenericMethod(sourceObject.Type, targetMember.Type),
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
                _mapEnumerableElementMethod.MakeGenericMethod(sourceElement.Type, targetElement.Type),
                sourceElement,
                targetElement,
                Parameters.EnumerableIndex);

            return mapCall;
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