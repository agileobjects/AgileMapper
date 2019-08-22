namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using NetStandardPolyfills;
    using Validation;

    internal class ObjectMappingData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IMappingContextOwner,
        IObjectMappingData,
        IObjectMappingData<TSource, TTarget>,
        IObjectCreationMappingData<TSource, TTarget, TTarget>
    {
        private ICache<IQualifiedMember, Func<TSource, Type>> _runtimeTypeGettersCache;
        private Dictionary<object, List<object>> _mappedObjectsBySource;
        private ObjectMapper<TSource, TTarget> _mapper;
        private ObjectMapperData _mapperData;

        public ObjectMappingData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            MappingTypes mappingTypes,
            IMappingContext mappingContext,
            IObjectMappingData parent,
            bool createMapper = true)
            : this(
                  source,
                  target,
                  enumerableIndex,
                  mappingTypes,
                  mappingContext,
                  null,
                  parent,
                  createMapper)
        {
        }

        private ObjectMappingData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            MappingTypes mappingTypes,
            IMappingContext mappingContext,
            IObjectMappingData declaredTypeMappingData,
            IObjectMappingData parent,
            bool createMapper)
            : base(source, target, enumerableIndex, parent, mappingContext)
        {
            MappingTypes = mappingTypes;
            MappingContext = mappingContext;
            DeclaredTypeMappingData = declaredTypeMappingData;

            if (parent != null)
            {
                Parent = parent;
                return;
            }

            if (createMapper)
            {
                _mapper = MapperContext.ObjectMapperFactory.GetOrCreateRoot(this);
            }
        }

        public IMappingContext MappingContext { get; }

        public MapperContext MapperContext => MappingContext.MapperContext;

        public MappingTypes MappingTypes { get; }

        public ObjectMapperKeyBase MapperKey { get; set; }

        public IRootMapperKey EnsureRootMapperKey()
        {
            MapperKey = MappingContext.RuleSet.RootMapperKeyFactory.Invoke(this);

            return (IRootMapperKey)MapperKey;
        }

        public IObjectMapper GetOrCreateMapper()
        {
            if (_mapper != null)
            {
                return _mapper;
            }

            _mapper = MapperContext.ObjectMapperFactory.Create(this);

            MapperKey.MappingData = null;

            if (_mapper == null)
            {
                return null;
            }

            if (MapperContext.UserConfigurations.ValidateMappingPlans)
            {
                // TODO: Test coverage for validation of standalone child mappers
                MappingValidator.Validate(_mapper.MapperData);
            }

            StaticMapperCache<TSource, TTarget>.AddIfAppropriate(_mapper, this);

            return _mapper;
        }

        public void SetMapper(IObjectMapper mapper)
            => _mapper = (ObjectMapper<TSource, TTarget>)mapper;

        public ObjectMapperData MapperData
        {
            get => _mapperData ?? (_mapperData = _mapper?.MapperData ?? ObjectMapperData.For(this));
            set => _mapperData = value;
        }

        public TTarget CreatedObject { get; set; }

        #region IObjectMappingData Members

        public bool IsRoot => Parent == null;

        public IObjectMappingData Parent { get; }

        IObjectMappingDataUntyped IObjectMappingData<TSource, TTarget>.Parent => Parent;

        public bool IsPartOfRepeatedMapping { get; set; }

        public bool IsPartOfDerivedTypeMapping => DeclaredTypeMappingData != null;

        public IObjectMappingData DeclaredTypeMappingData { get; }

        private Dictionary<object, List<object>> MappedObjectsBySource
            => _mappedObjectsBySource ?? (_mappedObjectsBySource = new Dictionary<object, List<object>>(13));

        IChildMemberMappingData IObjectMappingData.GetChildMappingData(IMemberMapperData childMapperData)
            => new ChildMemberMappingData<TSource, TTarget>(this, childMapperData);

        public Type GetSourceMemberRuntimeType(IQualifiedMember childSourceMember)
        {
            if (Source == null)
            {
                return childSourceMember.Type;
            }

            if (childSourceMember.Type.IsSealed())
            {
                return childSourceMember.Type;
            }

            var mapperData = MapperData;

            while (mapperData != null)
            {
                if (childSourceMember == mapperData.SourceMember)
                {
                    return mapperData.SourceMember.Type;
                }

                mapperData = mapperData.Parent;
            }

            if (_runtimeTypeGettersCache == null)
            {
                _runtimeTypeGettersCache = MapperContext.Cache.CreateScoped<IQualifiedMember, Func<TSource, Type>>();
            }

            var getRuntimeTypeFunc = _runtimeTypeGettersCache.GetOrAdd(childSourceMember, sm =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var relativeMember = sm.RelativeTo(MapperData.SourceMember);

                var memberAccess = relativeMember
                    .GetQualifiedAccess(MapperData)
                    .Replace(
                        MapperData.SourceObject,
                        sourceParameter,
                        ExpressionEvaluation.Equivalator);

                var getRuntimeTypeCall = Expression.Call(
                    ObjectExtensions.GetRuntimeSourceTypeMethod.MakeGenericMethod(sm.Type),
                    memberAccess);

                var getRuntimeTypeLambda = Expression
                    .Lambda<Func<TSource, Type>>(getRuntimeTypeCall, sourceParameter);

                return getRuntimeTypeLambda.Compile();
            });

            return getRuntimeTypeFunc.Invoke(Source);
        }

        #endregion

        #region Map Methods

        object IObjectMappingData.MapStart() => MapStart();

        public TTarget MapStart() => _mapper.Map(this);

        public TDeclaredTarget Map<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            string targetMemberRegistrationName,
            int dataSourceIndex)
        {
            var childMappingData = GetChildMappingData(
                sourceValue,
                targetValue,
                GetEnumerableIndex(),
                targetMemberRegistrationName,
                dataSourceIndex);

            return (TDeclaredTarget)_mapper.MapSubObject(childMappingData);
        }

        private IObjectMappingData GetChildMappingData<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex)
        {
            return ObjectMappingDataFactory.ForChild(
                sourceValue,
                targetValue,
                enumerableIndex,
                targetMemberRegistrationName,
                dataSourceIndex,
                this);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex)
        {
            var elementMappingData = GetElementMappingData(sourceElement, targetElement, enumerableIndex);

            return (TTargetElement)_mapper.MapSubObject(elementMappingData);
        }

        private IObjectMappingData GetElementMappingData<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex)
        {
            return ObjectMappingDataFactory.ForElement(
                sourceElement,
                targetElement,
                enumerableIndex,
                this);
        }

        TDeclaredTarget IObjectMappingDataUntyped.MapRepeated<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex)
        {
            if (IsRoot || MapperKey.MappingTypes.RuntimeTypesNeeded)
            {
                var childMappingData = GetChildMappingData(
                    sourceValue,
                    targetValue,
                    enumerableIndex,
                    targetMemberRegistrationName,
                    dataSourceIndex);

                childMappingData.IsPartOfRepeatedMapping = true;

                return (TDeclaredTarget)_mapper.MapRepeated(childMappingData);
            }

            return Parent.MapRepeated(
                sourceValue,
                targetValue,
                enumerableIndex,
                targetMemberRegistrationName,
                dataSourceIndex);
        }

        TDeclaredTarget IObjectMappingDataUntyped.MapRepeated<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceElement,
            TDeclaredTarget targetElement,
            int enumerableIndex)
        {
            if (IsRoot || MapperKey.MappingTypes.RuntimeTypesNeeded)
            {
                var childMappingData = GetElementMappingData(
                    sourceElement,
                    targetElement,
                    enumerableIndex);

                childMappingData.IsPartOfRepeatedMapping = true;

                return (TDeclaredTarget)_mapper.MapRepeated(childMappingData);
            }

            return Parent.MapRepeated(sourceElement, targetElement, enumerableIndex);
        }

        #endregion

        public bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
            where TComplex : class
        {
            if (!IsRoot)
            {
                return Parent.TryGet(key, out complexType);
            }

            if (MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
            {
                complexType = (TComplex)mappedTargets.FirstOrDefault(t => t is TComplex);
                return complexType != null;
            }

            complexType = default(TComplex);
            return false;
        }

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
        {
            if (!IsRoot)
            {
                Parent.Register(key, complexType);
                return;
            }

            if (MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
            {
                mappedTargets.Add(complexType);
                return;
            }

            _mappedObjectsBySource[key] = new List<object> { complexType };
        }

        public IObjectMappingData<TNewSource, TTarget> WithSource<TNewSource>(TNewSource newSource)
            => With(newSource, Target, isForDerivedTypeMapping: false);

        public IObjectMappingData<TNewSource, TNewTarget> WithSourceType<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewSource : class
        {
            return With(Source as TNewSource, default(TNewTarget), isForDerivedTypeMapping);
        }

        public IObjectMappingData<TNewSource, TNewTarget> WithTargetType<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewTarget : class
        {
            return With(default(TNewSource), Target as TNewTarget, isForDerivedTypeMapping);
        }

        public IObjectMappingData WithSource(IQualifiedMember newSourceMember)
        {
            var newSourceMappingData = WithTypes(
                GetSourceMemberRuntimeType(newSourceMember),
                MapperData.TargetType,
                IsPartOfDerivedTypeMapping);

            newSourceMappingData.MapperKey = MappingContext
                .RuleSet
                .RootMapperKeyFactory
                .Invoke(newSourceMappingData);

            newSourceMappingData.MapperData.OriginalMapperData = MapperData;
            newSourceMappingData.MapperData.Context.IsForToTargetMapping = true;

            return newSourceMappingData;
        }

        public IObjectMappingData WithTypes(Type newSourceType, Type newTargetType, bool isForDerivedTypeMapping)
        {
            var typesKey = new SourceAndTargetTypesKey(newSourceType, newTargetType);

            var typedAsCaller = GlobalContext.Instance.Cache.GetOrAdd(typesKey, k =>
            {
                var mappingDataParameter = Parameters.Create<IObjectMappingData<TSource, TTarget>>("mappingData");
                var isForDerivedTypeParameter = Parameters.Create<bool>("isForDerivedType");
                var withTypesCall = mappingDataParameter.GetAsCall(isForDerivedTypeParameter, k.SourceType, k.TargetType);

                var withTypesLambda = Expression
                    .Lambda<Func<IObjectMappingData<TSource, TTarget>, bool, IObjectMappingDataUntyped>>(
                        withTypesCall,
                        mappingDataParameter,
                        isForDerivedTypeParameter);

                return withTypesLambda.Compile();
            },
            default(HashCodeComparer<SourceAndTargetTypesKey>));

            return (IObjectMappingData)typedAsCaller.Invoke(this, isForDerivedTypeMapping);
        }

        public IObjectMappingData<TNewSource, TNewTarget> As<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewSource : class
            where TNewTarget : class
        {
            return With(Source as TNewSource, Target as TNewTarget, isForDerivedTypeMapping);
        }

        private IObjectMappingData<TNewSource, TNewTarget> With<TNewSource, TNewTarget>(
            TNewSource typedSource,
            TNewTarget typedTarget,
            bool isForDerivedTypeMapping)
        {
            if (MapperKey == null)
            {
                EnsureRootMapperKey();
            }

            var forceNewKey = isForDerivedTypeMapping && MapperKey.MappingTypes.TargetType.IsInterface();
            var mapperKey = MapperKey.WithTypes(typeof(TNewSource), typeof(TNewTarget), forceNewKey);

            return new ObjectMappingData<TNewSource, TNewTarget>(
                typedSource,
                typedTarget,
                GetEnumerableIndex(),
                mapperKey.MappingTypes,
                MappingContext,
                isForDerivedTypeMapping ? this : null,
                Parent,
                createMapper: false)
            {
                MapperKey = mapperKey
            };
        }
    }
}