namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using DataSources;
    using Enumerables;
    using Extensions;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using Members.Extensions;
    using Members.Sources;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class ObjectMapperData : MemberMapperDataBase, IMemberMapperData
    {
        private static readonly MethodInfo _mapRepeatedChildMethod =
            typeof(IObjectMappingDataUntyped).GetPublicInstanceMethod("MapRepeated", parameterCount: 6);

        private static readonly MethodInfo _mapRepeatedElementMethod =
            typeof(IObjectMappingDataUntyped).GetPublicInstanceMethod("MapRepeated", parameterCount: 4);

        private LabelTarget _returnLabelTarget;
        private Expression _rootMappingDataObject;
        private ObjectMapperData _entryPointMapperData;
        private Expression _targetInstance;
        private ParameterExpression _instanceVariable;
        private ParameterExpression _createdObject;
        private MappedObjectCachingMode _mappedObjectCachingMode;
        private List<ObjectMapperData> _childMapperDatas;
        private List<ObjectMapperData> _derivedMapperDatas;
        private Dictionary<QualifiedMember, IDataSourceSet> _dataSourcesByTargetMember;
        private bool? _isRepeatMapping;

        private ObjectMapperData(
            IMappingContext mappingContext,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            int? dataSourceIndex,
            ObjectMapperData declaredTypeMapperData,
            ObjectMapperData parent,
            bool isForStandaloneMapping)
            : base(
                mappingContext.RuleSet,
                sourceMember,
                targetMember,
                parent,
                mappingContext.MapperContext)
        {
            DataSourceIndex = dataSourceIndex.GetValueOrDefault();

            // TODO
            //CreatedObject = GetMappingDataProperty(nameof(CreatedObject));

            var isPartOfDerivedTypeMapping = declaredTypeMapperData != null;

            if (isPartOfDerivedTypeMapping)
            {
                DeclaredTypeMapperData = OriginalMapperData = declaredTypeMapperData;
                ElementIndex = declaredTypeMapperData.ElementIndex;
                ElementKey = declaredTypeMapperData.ElementKey;
                ParentObject = declaredTypeMapperData.ParentObject;
                declaredTypeMapperData.DerivedMapperDatas.Add(this);
            }
            else
            {
                ElementIndex = GetElementIndexAccess();
                ElementKey = GetElementKeyAccess();
                ParentObject = GetParentObjectAccess();
            }

            _mappedObjectCachingMode = MapperContext.UserConfigurations.CacheMappedObjects(this);

            if (targetMember.IsEnumerable)
            {
                EnumerablePopulationBuilder =
                    new EnumerablePopulationBuilder(this, TargetEnumerableVariableCreated);
            }

            if (IsRoot)
            {
                TargetTypeWillNotBeMappedAgain = IsTargetTypeLastMapping(parent);
                Context = new MapperDataContext(this, true, isPartOfDerivedTypeMapping);
                return;
            }

            parent.ChildMapperDatas.Add(this);

            if (this.TargetMemberIsEnumerableElement())
            {
                TargetTypeHasBeenMappedBefore = true;
            }
            else
            {
                TargetTypeHasBeenMappedBefore = IsNotTargetTypeFirstMapping(parent);
                TargetTypeWillNotBeMappedAgain = IsTargetTypeLastMapping(parent);
            }

            Context = new MapperDataContext(
                this,
                isForStandaloneMapping,
                isPartOfDerivedTypeMapping || parent.Context.IsForDerivedType);
        }

        #region Setup

        private bool IsNotTargetTypeFirstMapping(ObjectMapperData parent)
        {
            if (IsRepeatMapping)
            {
                return true;
            }

            while (parent != null)
            {
                if (parent.TargetTypeHasBeenMappedBefore)
                {
                    return true;
                }

                if (parent.HasTypeBeenMapped(TargetType, this))
                {
                    return true;
                }

                parent = parent.Parent;
            }

            return false;
        }

        private bool HasTypeBeenMapped(Type targetType, IQualifiedMemberContext requestingMapperData)
        {
            var mappedType = TargetMember.IsEnumerable ? TargetMember.ElementType : TargetType;

            if (targetType.IsAssignableTo(mappedType))
            {
                return true;
            }

            foreach (var childMapperData in ChildMapperDatasOrEmpty)
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

        private void TargetEnumerableVariableCreated(ParameterExpression targetVariable)
        {
            _instanceVariable = targetVariable;
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
                mappingData.MappingContext,
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
            var membersSource = mappingData.MapperKey.GetMembersSource(parentMapperData);

            if (!(membersSource is IChildMembersSource childMembersSource))
            {
                existingMapperData = null;
                return false;
            }

            var mapperTypes = new[] { typeof(TSource), typeof(TTarget) };

            existingMapperData = GetMapperDataOrNull(
                parentMapperData,
                mapperTypes,
                childMembersSource.TargetMemberRegistrationName);

            return existingMapperData != null;
        }

        private static ObjectMapperData GetMapperDataOrNull(
            ObjectMapperData parentMapperData,
            IList<Type> mapperTypes,
            string targetMemberRegistrationName)
        {
            foreach (var mapperData in parentMapperData.ChildMapperDatasOrEmpty)
            {
                if ((mapperData.TypesMatch(mapperTypes)) &&
                    (mapperData.TargetMember.RegistrationName == targetMemberRegistrationName))
                {
                    return mapperData;
                }

                if (mapperData.HasChildMapperDatas)
                {
                    return GetMapperDataOrNull(
                        mapperData,
                        mapperTypes,
                        targetMemberRegistrationName);
                }
            }

            return null;
        }

        #endregion

        public IObjectMapper Mapper { get; set; }

        public ObjectMapperData DeclaredTypeMapperData { get; }

        public ObjectMapperData OriginalMapperData { get; set; }

        public bool HasChildMapperDatas => _childMapperDatas?.Count > 0;

        public bool AnyChildMapperDataMatches(Func<ObjectMapperData, bool> matcher)
            => _childMapperDatas?.Any(matcher) == true;

        public IList<ObjectMapperData> ChildMapperDatas
            => _childMapperDatas ??= new List<ObjectMapperData>();

        public IList<ObjectMapperData> ChildMapperDatasOrEmpty
            => _childMapperDatas ?? (IList<ObjectMapperData>)Enumerable<ObjectMapperData>.EmptyArray;

        public IList<ObjectMapperData> DerivedMapperDatas
            => _derivedMapperDatas ??= new List<ObjectMapperData>();

        public int DataSourceIndex { get; set; }

        public MapperDataContext Context { get; }

        public IQualifiedMember GetSourceMemberFor(string targetMemberRegistrationName, int dataSourceIndex)
        {
            var targetMember = GetTargetMember(targetMemberRegistrationName);

            return DataSourcesByTargetMember[targetMember][dataSourceIndex].SourceMember;
        }

        private QualifiedMember GetTargetMember(string targetMemberRegistrationName)
        {
            var targetMember = DataSourcesByTargetMember.Keys
                .FirstOrDefault(k => k.RegistrationName == targetMemberRegistrationName);

            return targetMember;
        }

        public QualifiedMember GetTargetMemberFor(string targetMemberRegistrationName)
            => TargetMember.GetChildMember(targetMemberRegistrationName);

        public Expression ParentObject { get; }

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

        public bool TargetTypeHasBeenMappedBefore { get; }

        private bool TargetTypeWillBeMappedAgain => !TargetTypeWillNotBeMappedAgain;

        public bool TargetTypeWillNotBeMappedAgain { get; }

        public Expression ElementIndex { get; }

        public Expression ElementKey { get; }

        protected override Expression GetNestedSourceObject()
        {
            return Parent.EnumerablePopulationBuilder?.SourceElement
                ?? base.GetNestedSourceObject();
        }

        protected override Expression GetNestedTargetObject()
        {
            if (EnumerablePopulationBuilder?.TargetVariable != null)
            {
                return EnumerablePopulationBuilder.TargetVariable;
            }

            var subjectMapperData = TargetMember.LeafMember.DeclaringType == TargetInstance.Type
                ? this : Parent;

            return TargetMember.GetAccess(subjectMapperData.TargetInstance, this);
        }

        public Expression TargetInstance
        {
            get => _targetInstance ??= GetTargetInstance();
            set => _targetInstance = value;
        }

        private Expression GetTargetInstance()
            => Context.UseLocalVariable ? LocalVariable : TargetObject;

        public ParameterExpression LocalVariable
        {
            get => _instanceVariable ??= CreateInstanceVariable();
            set => _instanceVariable = value;
        }

        private ParameterExpression CreateInstanceVariable()
            => TargetType.GetOrCreateParameter(TargetType.GetVariableNameInCamelCase());

        public Expression RootMappingDataObject
            => _rootMappingDataObject ??= GetRootMappingDataObject();

        private Expression GetRootMappingDataObject()
        {
            return Context.IsForToTargetMapping
                ? OriginalMapperData.MappingDataObject
                : MappingDataObject;
        }

        public EnumerablePopulationBuilder EnumerablePopulationBuilder { get; }

        public bool UsesCreatedObject => _createdObject != null;

        public ParameterExpression CreatedObject => _createdObject ??= CreateCreatedObject();

        private ParameterExpression CreateCreatedObject()
        {
            return TargetType.GetOrCreateParameter(
                "created" + TargetType.GetVariableNameInPascalCase());
        }

        public ObjectMapperData EntryPointMapperData
            => _entryPointMapperData ??= GetNearestEntryPointObjectMapperData();

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

        public override bool IsEntryPoint
            => IsRoot || Context.IsStandalone || IsRepeatMapping;

        public bool IsRepeatMapping => _isRepeatMapping ??= this.IsRepeatMapping();

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

        public Dictionary<QualifiedMember, IDataSourceSet> DataSourcesByTargetMember
            => _dataSourcesByTargetMember ??= new Dictionary<QualifiedMember, IDataSourceSet>();

        public Expression GetRuntimeTypedMapping(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            if (IsSimpleTypeToObjectMapping(sourceObject, targetMember.Type))
            {
                return sourceObject;
            }

            Context.RuntimeTypedMappingNeeded();

            const int MAP_CHILD_PARAMETER_COUNT = 7;

            Expression elementIndex, elementKey;

            if (IsRoot)
            {
                elementIndex = Constants.NullInt;
                elementKey = Constants.NullObject;
            }
            else
            {
                elementIndex = Parent.ElementIndex;
                elementKey = Parent.ElementKey;
            }

            var mapCall = Expression.Call(
                Constants.ExecutionContextParameter,
                GetMapMethod(MAP_CHILD_PARAMETER_COUNT)
                    .MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(this),
                elementIndex,
                elementKey,
                targetMember.RegistrationName.ToConstantExpression(),
                dataSourceIndex.ToConstantExpression(),
                GetParentContext());

            return GetSimpleTypeCheckedMapCall(sourceObject, targetMember.Type, mapCall);
        }

        public Expression GetRuntimeTypedMapping(Expression sourceElement, Expression targetElement)
        {
            if (!TargetMember.IsEnumerable && this.TargetMemberIsEnumerableElement())
            {
                return Parent.GetRuntimeTypedMapping(sourceElement, targetElement);
            }

            if (IsSimpleTypeToObjectMapping(sourceElement, targetElement.Type))
            {
                return sourceElement;
            }

            Context.RuntimeTypedMappingNeeded();

            const int MAP_ELEMENT_PARAMETER_COUNT = 5;

            var mapCall = Expression.Call(
                Constants.ExecutionContextParameter,
                GetMapMethod(MAP_ELEMENT_PARAMETER_COUNT)
                    .MakeGenericMethod(sourceElement.Type, targetElement.Type),
                sourceElement,
                targetElement,
                EnumerablePopulationBuilder.Counter,
                EnumerablePopulationBuilder.GetElementKey(),
                GetParentContext());

            return GetSimpleTypeCheckedMapCall(sourceElement, targetElement.Type, mapCall);
        }

        private Expression GetParentContext()
        {
            // TODO
            return Constants.ExecutionContextParameter;
        }

        private static bool IsSimpleTypeToObjectMapping(Expression sourceObject, Type targetType)
            => sourceObject.Type.IsSimple() && (targetType == typeof(object));

        private static MethodInfo GetMapMethod(int parameterCount)
        {
            return typeof(IMappingExecutionContext)
                .GetPublicInstanceMethod(nameof(IMappingExecutionContext.Map), parameterCount);
        }

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

            if (targetMember.IsEnumerableElement())
            {
                mapRepeatedMethod = _mapRepeatedElementMethod;

                arguments = new[]
                {
                    mappingValues.SourceValue,
                    mappingValues.TargetValue,
                    mappingValues.ElementIndex,
                    mappingValues.ElementKey
                };
            }
            else
            {
                mapRepeatedMethod = _mapRepeatedChildMethod;

                arguments = new[]
                {
                    mappingValues.SourceValue,
                    mappingValues.TargetValue,
                    ElementIndex,
                    ElementKey,
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

        /// <summary>
        /// Creates a GotoExpression passing the given <paramref name="value"/> to this
        /// <see cref="ObjectMapperData" />'s LabelTarget.
        /// </summary>
        /// <param name="value">The vlaue to pass to this <see cref="ObjectMapperData" />'s LabelTarget.</param>
        /// <returns>
        /// Aa GotoExpression passing the given <paramref name="value"/> to this
        /// <see cref="ObjectMapperData" />'s LabelTarget.
        /// </returns>
        public Expression GetReturnExpression(Expression value)
            => Expression.Return(ReturnLabelTarget, value, TargetType);

        /// <summary>
        /// Creates a LabelExpression for this <see cref="ObjectMapperData" />'s LabelTarget, with
        /// the given <paramref name="defaultValue"/>. The created LabelExpression marks the point
        /// in the compiled mapping Func to which execution will jump from GotoExpressions created
        /// by calls to this <see cref="ObjectMapperData" />'s GetReturnExpression() method.
        /// </summary>
        /// <param name="defaultValue">The default value of the LabelExpression to create.</param>
        /// <returns>
        /// A LabelExpression for this <see cref="ObjectMapperData" />'s LabelTarget, with the given
        /// <paramref name="defaultValue"/>.
        /// </returns>
        public Expression GetReturnLabel(Expression defaultValue)
            => Expression.Label(ReturnLabelTarget, defaultValue);

        private LabelTarget ReturnLabelTarget
            => _returnLabelTarget ??= Expression.Label(TargetType, "Return");

        public bool ReturnLabelUsed => _returnLabelTarget != null;

        public IQualifiedMemberContext WithNoTargetMember()
        {
            return new QualifiedMemberContext(
                RuleSet,
                SourceType,
                TargetType,
                SourceMember,
                QualifiedMember.None,
                Parent,
                MapperContext);
        }

        #region ToString
#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString() => SourceMember + " -> " + TargetMember;
#endif
        #endregion
    }
}