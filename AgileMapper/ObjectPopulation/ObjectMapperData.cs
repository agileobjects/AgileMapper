namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using DataSources;
    using Enumerables;
    using Extensions;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using Members.Sources;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static Members.Member;

    internal class ObjectMapperData : BasicMapperData, IMemberMapperData
    {
        private static readonly MethodInfo _mapRepeatedChildMethod =
            typeof(IObjectMappingDataUntyped).GetPublicInstanceMethod("MapRepeated", parameterCount: 5);

        private static readonly MethodInfo _mapRepeatedElementMethod =
            typeof(IObjectMappingDataUntyped).GetPublicInstanceMethod("MapRepeated", parameterCount: 3);

        private readonly List<ObjectMapperData> _childMapperDatas;
        private ObjectMapperData _entryPointMapperData;
        private Expression _targetInstance;
        private ParameterExpression _instanceVariable;
        private MappedObjectCachingMode _mappedObjectCachingMode;
        private bool? _isRepeatMapping;
        private bool _isEntryPoint;

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
                sourceMember,
                targetMember,
                parent)
        {
            MapperContext = mappingData.MappingContext.MapperContext;
            DeclaredTypeMapperData = OriginalMapperData = declaredTypeMapperData;
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

            DataSourcesByTargetMember = new Dictionary<QualifiedMember, DataSourceSet>();

            ReturnLabelTarget = Expression.Label(TargetType, "Return");
            _mappedObjectCachingMode = MapperContext.UserConfigurations.CacheMappedObjects(this);

            if (targetMember.IsEnumerable)
            {
                EnumerablePopulationBuilder = new EnumerablePopulationBuilder(this);
            }

            if (IsRoot)
            {
                TargetTypeHasNotYetBeenMapped = true;
                TargetTypeWillNotBeMappedAgain = IsTargetTypeLastMapping(parent);
                Context = new MapperDataContext(this, true, isPartOfDerivedTypeMapping);
                return;
            }

            parent._childMapperDatas.Add(this);
            Parent = parent;

            if (!this.TargetMemberIsEnumerableElement())
            {
                TargetTypeHasNotYetBeenMapped = IsTargetTypeFirstMapping(parent);
                TargetTypeWillNotBeMappedAgain = IsTargetTypeLastMapping(parent);
            }

            Context = new MapperDataContext(
                this,
                isForStandaloneMapping,
                isPartOfDerivedTypeMapping || parent.Context.IsForDerivedType);
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

        private bool IsTargetTypeFirstMapping(ObjectMapperData parent)
        {
            if (IsRepeatMapping)
            {
                return false;
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
            if (IsRepeatMapping)
            {
                return false;
            }

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

            if (parentType.IsEnumerable())
            {
                parentType = parentType.GetEnumerableElementType();

                return !parentType.IsSimple() && TypeHasACompatibleChildMember(targetType, parentType, checkedTypes);
            }

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

        public static ObjectMapperData For<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            int? dataSourceIndex;
            ObjectMapperData parentMapperData;
            IMembersSource membersSource;

            if (mappingData.IsRoot)
            {
                parentMapperData = null;
                membersSource = mappingData.MappingContext.MapperContext.RootMembersSource;
                dataSourceIndex = null;
            }
            else if (UseExistingMapperData(mappingData, out var existingMapperData))
            {
                return existingMapperData;
            }
            else
            {
                parentMapperData = mappingData.Parent.MapperData;
                membersSource = mappingData.MapperKey.GetMembersSource(parentMapperData);
                dataSourceIndex = membersSource.DataSourceIndex;
            }

            var sourceMember = membersSource.GetSourceMember<TSource, TTarget>().WithType(typeof(TSource));
            var targetMember = membersSource.GetTargetMember<TSource, TTarget>().WithType(typeof(TTarget));

            return new ObjectMapperData(
                mappingData,
                sourceMember,
                targetMember,
                dataSourceIndex,
                mappingData.DeclaredTypeMappingData?.MapperData,
                parentMapperData,
                mappingData.IsStandalone());
        }

        private static bool UseExistingMapperData<TSource, TTarget>(
            ObjectMappingData<TSource, TTarget> mappingData,
            out ObjectMapperData existingMapperData)
        {
            if (!mappingData.IsPartOfRepeatedMapping)
            {
                existingMapperData = null;
                return false;
            }

            // RepeatedMappings are invoked through the entry point MapperData, which assigns
            // itself as the ObjectMappingData's parent. If a repeated mapping then needs to
            // do a runtime-typed child mapping, we're able to reuse the parent MapperData 
            // by finding it from the entry point:
            var parentMapperData = mappingData.Parent.MapperData;

            if (parentMapperData.ChildMapperDatas.None())
            {
                existingMapperData = null;
                return false;
            }

            var membersSource = mappingData.MapperKey.GetMembersSource(parentMapperData);

            if (!(membersSource is IChildMembersSource childMembersSource))
            {
                existingMapperData = null;
                return false;
            }

            var mapperTypes = new[] { typeof(TSource), typeof(TTarget) };

            existingMapperData = GetMapperDataOrNull(
                parentMapperData.ChildMapperDatas,
                mapperTypes,
                childMembersSource.TargetMemberRegistrationName);

            return existingMapperData != null;
        }

        private static ObjectMapperData GetMapperDataOrNull(
            IEnumerable<ObjectMapperData> mapperDatas,
            IList<Type> mapperTypes,
            string targetMemberRegistrationName)
        {
            foreach (var mapperData in mapperDatas)
            {
                if ((mapperData.TypesMatch(mapperTypes)) &&
                    (mapperData.TargetMember.RegistrationName == targetMemberRegistrationName))
                {
                    return mapperData;
                }

                if (mapperData.ChildMapperDatas.Any())
                {
                    return GetMapperDataOrNull(
                        mapperData.ChildMapperDatas,
                        mapperTypes,
                        targetMemberRegistrationName);
                }
            }

            return null;
        }

        #endregion

        public MapperContext MapperContext { get; }

        public IObjectMapper Mapper { get; set; }

        public ObjectMapperData Parent { get; }

        public ObjectMapperData DeclaredTypeMapperData { get; }

        public ObjectMapperData OriginalMapperData { get; set; }

        public IList<ObjectMapperData> ChildMapperDatas => _childMapperDatas;

        public int DataSourceIndex { get; set; }

        public MapperDataContext Context { get; }

        public override bool HasCompatibleTypes(ITypePair typePair)
            => typePair.HasCompatibleTypes(this, SourceMember, TargetMember);

        public IQualifiedMember GetSourceMemberFor(string targetMemberRegistrationName, int dataSourceIndex)
        {
            var targetMember = GetTargetMember(targetMemberRegistrationName, this);

            return DataSourcesByTargetMember[targetMember][dataSourceIndex].SourceMember;
        }

        private static QualifiedMember GetTargetMember(
            string targetMemberRegistrationName,
            ObjectMapperData mapperData)
        {
            var targetMember = mapperData
                .DataSourcesByTargetMember
                .Keys
                .FirstOrDefault(k => k.RegistrationName == targetMemberRegistrationName);

            return targetMember;
        }

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

        public Expression SourceObject { get; set; }

        public Expression TargetObject { get; set; }

        public Expression EnumerableIndex { get; }

        public Expression TargetInstance
        {
            get => _targetInstance ?? (_targetInstance = GetTargetInstance());
            set => _targetInstance = value;
        }

        private Expression GetTargetInstance()
            => Context.UseLocalVariable ? LocalVariable : TargetObject;

        public ParameterExpression LocalVariable
        {
            get => _instanceVariable ?? (_instanceVariable = CreateInstanceVariable());
            set => _instanceVariable = value;
        }

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
            var mapperData = GetEntryPointMapperDataCandidate(this);

            while (!mapperData.IsEntryPoint)
            {
                mapperData = GetEntryPointMapperDataCandidate(mapperData.Parent);
            }

            return mapperData;
        }

        private static ObjectMapperData GetEntryPointMapperDataCandidate(ObjectMapperData mapperData)
        {
            if ((mapperData.OriginalMapperData == null) ||
               (!mapperData.OriginalMapperData.IsEntryPoint && mapperData.IsEntryPoint))
            {
                return mapperData;
            }

            return mapperData.OriginalMapperData;
        }

        public bool IsEntryPoint
        {
            get => _isEntryPoint || IsRoot || Context.IsStandalone || IsRepeatMapping;
            set => _isEntryPoint = value;
        }

        public bool IsRepeatMapping => (_isRepeatMapping ?? (_isRepeatMapping = this.IsRepeatMapping())).Value;

        public void RegisterRepeatedMapperFunc(IObjectMappingData mappingData)
        {
            var nearestStandaloneMapperData = GetNearestStandaloneMapperData();

            if (nearestStandaloneMapperData.RepeatedMapperFuncKeys == null)
            {
                nearestStandaloneMapperData.RepeatedMapperFuncKeys = new List<ObjectMapperKeyBase>();
            }
            else if (nearestStandaloneMapperData.RepeatedMapperFuncKeys.Contains(mappingData.MapperKey))
            {
                return;
            }

            mappingData.MapperKey.MapperData = mappingData.MapperData;
            mappingData.MapperKey.MappingData = mappingData;

            nearestStandaloneMapperData.RepeatedMapperFuncKeys.Add(mappingData.MapperKey);
        }

        public ObjectMapperData GetNearestStandaloneMapperData()
        {
            var mapperData = this;

            while (!mapperData.Context.IsStandalone)
            {
                mapperData = mapperData.Parent;
            }

            return mapperData;
        }

        public bool HasRepeatedMapperFuncs => RepeatedMapperFuncKeys?.Any() == true;

        public IList<ObjectMapperKeyBase> RepeatedMapperFuncKeys { get; private set; }

        public Dictionary<QualifiedMember, DataSourceSet> DataSourcesByTargetMember { get; }

        public Expression GetMapCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            Context.SubMappingNeeded();

            var mapCall = Expression.Call(
                MappingDataObject,
                GetMapMethod(MappingDataObject.Type, 4)
                    .MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(this),
                targetMember.RegistrationName.ToConstantExpression(),
                dataSourceIndex.ToConstantExpression());

            return GetSimpleTypeCheckedMapCall(sourceObject, targetMember.Type, mapCall);
        }

        public Expression GetMapCall(Expression sourceElement, Expression targetElement)
        {
            if (!TargetMember.IsEnumerable && this.TargetMemberIsEnumerableElement())
            {
                return Parent.GetMapCall(sourceElement, targetElement);
            }

            Context.SubMappingNeeded();

            var mapCall = Expression.Call(
                MappingDataObject,
                GetMapMethod(MappingDataObject.Type, 3)
                    .MakeGenericMethod(sourceElement.Type, targetElement.Type),
                sourceElement,
                targetElement,
                EnumerablePopulationBuilder.Counter);

            return GetSimpleTypeCheckedMapCall(sourceElement, targetElement.Type, mapCall);
        }

        private static MethodInfo GetMapMethod(Type mappingDataType, int numberOfArguments)
            => mappingDataType.GetPublicInstanceMethod("Map", numberOfArguments);

        private static Expression GetSimpleTypeCheckedMapCall(
            Expression sourceObject,
            Type targetObjectType,
            Expression mapCall)
        {
            if ((sourceObject.Type != typeof(object)) || (targetObjectType != typeof(object)))
            {
                return mapCall;
            }

            var sourceObjectGetTypeMethod = typeof(object).GetPublicInstanceMethod(nameof(GetType));
            var sourceObjectGetTypeCall = Expression.Call(sourceObject, sourceObjectGetTypeMethod);
            var isSimpleMethod = Extensions.PublicTypeExtensions.IsSimpleMethod;
            var sourceObjectTypeIsSimpleCall = Expression.Call(isSimpleMethod, sourceObjectGetTypeCall);

            var simpleSourceTypeOrMapCall = Expression.Condition(
                sourceObjectTypeIsSimpleCall,
                sourceObject,
                mapCall);

            return simpleSourceTypeOrMapCall;
        }

        public MethodCallExpression GetMapRepeatedCall(
            QualifiedMember targetMember,
            MappingValues mappingValues,
            int dataSourceIndex)
        {
            MethodInfo mapRepeatedMethod;
            Expression[] arguments;

            if (targetMember.LeafMember.IsEnumerableElement())
            {
                mapRepeatedMethod = _mapRepeatedElementMethod;

                arguments = new[]
                {
                    mappingValues.SourceValue,
                    mappingValues.TargetValue,
                    mappingValues.EnumerableIndex,
                };
            }
            else
            {
                mapRepeatedMethod = _mapRepeatedChildMethod;

                arguments = new[]
                {
                    mappingValues.SourceValue,
                    mappingValues.TargetValue,
                    EnumerableIndex,
                    targetMember.RegistrationName.ToConstantExpression(),
                    dataSourceIndex.ToConstantExpression()
                };
            }

            mapRepeatedMethod = mapRepeatedMethod.MakeGenericMethod(
                mappingValues.SourceValue.Type,
                mappingValues.TargetValue.Type);

            var mapRepeatedCall = Expression.Call(
                EntryPointMapperData.MappingDataObject,
                mapRepeatedMethod,
                arguments);

            return mapRepeatedCall;
        }

        public IBasicMapperData WithNoTargetMember()
        {
            return new BasicMapperData(
                RuleSet,
                SourceType,
                TargetType,
                SourceMember,
                QualifiedMember.None,
                Parent);
        }

        #region ToString
#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString() => SourceMember + " -> " + TargetMember;
#endif
        #endregion
    }
}