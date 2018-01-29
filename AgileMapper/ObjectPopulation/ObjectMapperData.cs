namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Enumerables;
    using Extensions.Internal;
    using Members;
    using Members.Sources;
    using NetStandardPolyfills;
    using static Members.Member;

    internal class ObjectMapperData : BasicMapperData, IMemberMapperData
    {
        private static readonly MethodInfo _mapRecursionMethod =
            typeof(IObjectMappingDataUntyped).GetPublicInstanceMethod("MapRecursion");

        private readonly List<ObjectMapperData> _childMapperDatas;
        private readonly MethodInfo _mapChildMethod;
        private readonly MethodInfo _mapElementMethod;
        private readonly Dictionary<string, DataSourceSet> _dataSourcesByTargetMemberName;
        private ObjectMapperData _entryPointMapperData;
        private Expression _targetInstance;
        private ParameterExpression _instanceVariable;
        private MappedObjectCachingMode _mappedObjectCachingMode;

        private ObjectMapperData(
            IObjectMappingData mappingData,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            int? dataSourceIndex,
            ObjectMapperData declaredTypeMapperData,
            ObjectMapperData parent,
            bool isForStandaloneMapping)
            : base(
                  mappingData.MappingContext.RuleSet,
                  sourceMember.Type,
                  targetMember.Type,
                  targetMember,
                  parent)
        {
            MapperContext = mappingData.MappingContext.MapperContext;
            DeclaredTypeMapperData = declaredTypeMapperData;
            _childMapperDatas = new List<ObjectMapperData>();
            DataSourceIndex = dataSourceIndex.GetValueOrDefault();

            MappingDataObject = GetMappingDataObject(parent);
            SourceMember = sourceMember;

            var mappingDataType = typeof(IMappingData<,>).MakeGenericType(SourceType, TargetType);
            SourceObject = GetMappingDataProperty(mappingDataType, RootSourceMemberName);
            TargetObject = GetMappingDataProperty(RootTargetMemberName);
            CreatedObject = GetMappingDataProperty("CreatedObject");

            var isPartOfDerivedTypeMapping = declaredTypeMapperData != null;

            if (isPartOfDerivedTypeMapping)
            {
                EnumerableIndex = declaredTypeMapperData.EnumerableIndex;
                ParentObject = declaredTypeMapperData.ParentObject;
            }
            else
            {
                EnumerableIndex = GetMappingDataProperty(mappingDataType, "EnumerableIndex");
                ParentObject = GetMappingDataProperty("Parent");
            }

            ExpressionInfoFinder = new ExpressionInfoFinder(MappingDataObject);

            _mapChildMethod = GetMapMethod(MappingDataObject.Type, 4);
            _mapElementMethod = GetMapMethod(MappingDataObject.Type, 3);

            _dataSourcesByTargetMemberName = new Dictionary<string, DataSourceSet>();
            DataSourcesByTargetMember = new Dictionary<QualifiedMember, DataSourceSet>();

            if (targetMember.IsEnumerable)
            {
                EnumerablePopulationBuilder = new EnumerablePopulationBuilder(this);
            }
            else
            {
                if (!this.TargetMemberIsEnumerableElement())
                {
                    TargetTypeHasNotYetBeenMapped = IsTargetTypeFirstMapping(parent);
                    TargetTypeWillNotBeMappedAgain = IsTargetTypeLastMapping(parent);
                }
            }

            ReturnLabelTarget = Expression.Label(TargetType, "Return");
            _mappedObjectCachingMode = MapperContext.UserConfigurations.CacheMappedObjects(this);

            if (isForStandaloneMapping)
            {
                RequiredMapperFuncsByKey = new Dictionary<ObjectMapperKeyBase, LambdaExpression>();
            }

            if (IsRoot)
            {
                Context = new MapperDataContext(this, true, isPartOfDerivedTypeMapping);
                return;
            }

            Context = new MapperDataContext(
                this,
                isForStandaloneMapping,
                isPartOfDerivedTypeMapping || parent.Context.IsForDerivedType);

            parent._childMapperDatas.Add(this);
            Parent = parent;
        }

        #region Setup

        private ParameterExpression GetMappingDataObject(ObjectMapperData parent)
        {
            var mdType = typeof(IObjectMappingData<,>).MakeGenericType(SourceType, TargetType);

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

            var parameter = Expression.Parameter(mdType, mappingDataVariableName);

            return parameter;
        }

        private Expression GetMappingDataProperty(Type mappingDataType, string propertyName)
        {
            var property = mappingDataType.GetPublicInstanceProperty(propertyName);

            return Expression.Property(MappingDataObject, property);
        }

        private Expression GetMappingDataProperty(string propertyName)
            => Expression.Property(MappingDataObject, propertyName);

        private static MethodInfo GetMapMethod(Type mappingDataType, int numberOfArguments)
        {
            return mappingDataType
                .GetPublicInstanceMethod("Map", parameterCount: numberOfArguments);
        }

        private bool IsTargetTypeFirstMapping(ObjectMapperData parent)
        {
            if (IsRoot)
            {
                return true;
            }

            while (parent != null)
            {
                if (parent.TargetTypeHasBeenMappedBefore)
                {
                    return false;
                }

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

            if (targetType.IsAssignableTo(mappedType))
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

        private bool IsTargetTypeLastMapping(ObjectMapperData parent)
        {
            while (parent != null)
            {
                if (parent.TargetTypeWillBeMappedAgain)
                {
                    return false;
                }

                parent = parent.Parent;
            }

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

            var childTargetMembers = GlobalContext.Instance.MemberCache.GetTargetMembers(parentType);

            foreach (var childMember in childTargetMembers)
            {
                if (childMember.IsSimple)
                {
                    continue;
                }

                if (childMember.IsComplex)
                {
                    if (targetType.IsAssignableTo(childMember.Type))
                    {
                        return true;
                    }

                    if (TypeHasACompatibleChildMember(targetType, childMember.Type, checkedTypes))
                    {
                        return true;
                    }

                    continue;
                }

                if (childMember.ElementType.IsComplex() && targetType.IsAssignableTo(childMember.ElementType))
                {
                    return true;
                }

                if (TypeHasACompatibleChildMember(targetType, childMember.ElementType, checkedTypes))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Factory Method

        public static ObjectMapperData For<TSource, TTarget>(IObjectMappingData mappingData)
        {
            var membersSource = mappingData.MapperKey.GetMembersSource(mappingData.Parent);
            var sourceMember = membersSource.GetSourceMember<TSource, TTarget>().WithType(typeof(TSource));
            var targetMember = membersSource.GetTargetMember<TSource, TTarget>().WithType(typeof(TTarget));

            int? dataSourceIndex;
            ObjectMapperData parentMapperData;

            if (mappingData.IsRoot)
            {
                dataSourceIndex = null;
                parentMapperData = null;
            }
            else
            {
                dataSourceIndex = (membersSource as IChildMembersSource)?.DataSourceIndex;
                parentMapperData = mappingData.Parent.MapperData;
            }

            return new ObjectMapperData(
                mappingData,
                sourceMember,
                targetMember,
                dataSourceIndex,
                mappingData.DeclaredTypeMappingData?.MapperData,
                parentMapperData,
                mappingData.IsStandalone());
        }

        #endregion

        public MapperContext MapperContext { get; }

        public IObjectMapper Mapper { get; set; }

        public ObjectMapperData Parent { get; }

        public ObjectMapperData DeclaredTypeMapperData { get; }

        public IList<ObjectMapperData> ChildMapperDatas => _childMapperDatas;

        public int DataSourceIndex { get; set; }

        public MapperDataContext Context { get; }

        public override bool HasCompatibleTypes(ITypePair typePair)
        {
            return typePair.HasCompatibleTypes(
                this,
                () => SourceMember.HasCompatibleType(typePair.SourceType),
                () => TargetMember.HasCompatibleType(typePair.TargetType));
        }

        public IQualifiedMember GetSourceMemberFor(string targetMemberRegistrationName, int dataSourceIndex)
            => _dataSourcesByTargetMemberName[targetMemberRegistrationName][dataSourceIndex].SourceMember;

        public QualifiedMember GetTargetMemberFor(string targetMemberRegistrationName)
            => TargetMember.GetChildMember(targetMemberRegistrationName);

        public ParameterExpression MappingDataObject { get; }

        public Expression ParentObject { get; }

        public IQualifiedMember SourceMember { get; }

        public bool CacheMappedObjects
        {
            get => _mappedObjectCachingMode == MappedObjectCachingMode.Cache;
            set
            {
                if (_mappedObjectCachingMode == MappedObjectCachingMode.DoNotCache)
                {
                    return;
                }

                if (value == false)
                {
                    _mappedObjectCachingMode = MappedObjectCachingMode.DoNotCache;
                    return;
                }

                _mappedObjectCachingMode = MappedObjectCachingMode.Cache;

                if (!IsRoot)
                {
                    Parent.CacheMappedObjects = true;
                }
            }
        }

        private bool TargetTypeHasBeenMappedBefore => !TargetTypeHasNotYetBeenMapped;

        public bool TargetTypeHasNotYetBeenMapped { get; }

        private bool TargetTypeWillBeMappedAgain => !TargetTypeWillNotBeMappedAgain;

        public bool TargetTypeWillNotBeMappedAgain { get; }

        public Expression SourceObject { get; }

        public Expression TargetObject { get; }

        public Expression EnumerableIndex { get; }

        public Expression TargetInstance
            => _targetInstance ?? (_targetInstance = GetTargetInstance());

        private Expression GetTargetInstance()
            => Context.UseLocalVariable ? LocalVariable : TargetObject;

        public ParameterExpression LocalVariable
            => _instanceVariable ?? (_instanceVariable = CreateInstanceVariable());

        private ParameterExpression CreateInstanceVariable()
        {
            return EnumerablePopulationBuilder?.TargetVariable
                ?? Expression.Variable(TargetType, TargetType.GetVariableNameInCamelCase());
        }

        public ExpressionInfoFinder ExpressionInfoFinder { get; }

        public EnumerablePopulationBuilder EnumerablePopulationBuilder { get; }

        public Expression CreatedObject { get; }

        public LabelTarget ReturnLabelTarget { get; }

        public Expression GetReturnLabel(Expression defaultValue)
            => Expression.Label(ReturnLabelTarget, defaultValue);

        public ObjectMapperData EntryPointMapperData
            => _entryPointMapperData ?? (_entryPointMapperData = GetNearestEntryPointObjectMapperData());

        private ObjectMapperData GetNearestEntryPointObjectMapperData()
        {
            var mapperData = DeclaredTypeMapperData ?? this;

            while (!mapperData.IsEntryPoint())
            {
                mapperData = mapperData.Parent.DeclaredTypeMapperData ?? mapperData.Parent;
            }

            return mapperData;
        }

        public bool IsEntryPoint()
        {
            if (IsRoot || Context.IsStandalone)
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

            if (mappingLambda != null)
            {
                // The mapping lambda can be null if it turns out the nested mapping 
                // function has all-unmappable members, i.e. it doesn't map anything:
                nearestStandaloneMapperData.RequiredMapperFuncsByKey[mappingData.MapperKey] = mappingLambda;
            }
        }

        private ObjectMapperData GetNearestStandaloneMapperData()
        {
            var mapperData = this;

            while (!mapperData.Context.IsStandalone)
            {
                mapperData = mapperData.Parent;
            }

            return mapperData;
        }

        public bool HasMapperFuncs => RequiredMapperFuncsByKey?.Any(f => f.Value != null) == true;

        public Dictionary<ObjectMapperKeyBase, LambdaExpression> RequiredMapperFuncsByKey { get; }

        public Dictionary<QualifiedMember, DataSourceSet> DataSourcesByTargetMember { get; }

        public MethodCallExpression GetMapCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            Context.SubMappingNeeded();

            var mapCall = Expression.Call(
                MappingDataObject,
                _mapChildMethod.MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(this),
                targetMember.RegistrationName.ToConstantExpression(),
                dataSourceIndex.ToConstantExpression());

            return mapCall;
        }

        public MethodCallExpression GetMapCall(Expression sourceElement)
            => GetMapCall(sourceElement, TargetMember.ElementType.ToDefaultExpression());

        public MethodCallExpression GetMapCall(Expression sourceElement, Expression targetElement)
        {
            if (!TargetMember.IsEnumerable && this.TargetMemberIsEnumerableElement())
            {
                return Parent.GetMapCall(sourceElement, targetElement);
            }

            Context.SubMappingNeeded();

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
            var mapCall = Expression.Call(
                EntryPointMapperData.MappingDataObject,
                _mapRecursionMethod.MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(this),
                EnumerableIndex,
                targetMember.RegistrationName.ToConstantExpression(),
                dataSourceIndex.ToConstantExpression());

            return mapCall;
        }

        public void RegisterTargetMemberDataSourcesIfRequired(QualifiedMember targetMember, DataSourceSet dataSources)
        {
            // TODO: Only add entries where necessary
            DataSourcesByTargetMember.Add(targetMember, dataSources);

            if (targetMember.IsSimple)
            {
                return;
            }

            // TODO: Only add entries where necessary
            _dataSourcesByTargetMemberName.Add(targetMember.RegistrationName, dataSources);
        }

        public IBasicMapperData WithNoTargetMember()
            => new BasicMapperData(RuleSet, SourceType, TargetType, QualifiedMember.None, Parent);

        #region ToString
#if CODE_COVERAGE_SUPPORTED
        [ExcludeFromCodeCoverage]
        public override string ToString() => SourceMember + " -> " + TargetMember;
#endif
        #endregion
    }
}