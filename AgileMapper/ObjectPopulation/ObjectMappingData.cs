namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class ObjectMappingData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IObjectMappingData,
        IObjectMappingData<TSource, TTarget>,
        IObjectCreationMappingData<TSource, TTarget, TTarget>
    {
        private readonly Dictionary<object, List<object>> _mappedObjectsBySource;
        private ObjectMapper<TSource, TTarget> _mapper;
        private ObjectMapperData _mapperData;

        public ObjectMappingData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            ObjectMapperKeyBase mapperKey,
            IMappingContext mappingContext,
            IObjectMappingData parent)
            : this(
                  source,
                  target,
                  enumerableIndex,
                  mapperKey,
                  mappingContext,
                  null,
                  parent)
        {
        }

        private ObjectMappingData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            ObjectMapperKeyBase mapperKey,
            IMappingContext mappingContext,
            IObjectMappingData declaredTypeMappingData,
            IObjectMappingData parent)
            : base(source, target, enumerableIndex, parent)
        {
            MapperKey = mapperKey;
            MappingContext = mappingContext;
            DeclaredTypeMappingData = declaredTypeMappingData;

            if (parent != null)
            {
                Parent = parent;
                return;
            }

            if (IsPartOfDerivedTypeMapping)
            {
                return;
            }

            _mapper = MapperContext.ObjectMapperFactory.GetOrCreateRoot(this);

            if (MapperData.MappedObjectCachingNeeded)
            {
                _mappedObjectsBySource = new Dictionary<object, List<object>>(13);
            }
        }

        public IMappingContext MappingContext { get; }

        public MapperContext MapperContext => MappingContext.MapperContext;

        public ObjectMapperKeyBase MapperKey { get; }

        public IObjectMapper Mapper
        {
            get => _mapper ?? (_mapper = MapperContext.ObjectMapperFactory.Create(this));
            set
            {
                _mapper = (ObjectMapper<TSource, TTarget>)value;
                _mapperData = _mapper.MapperData;
            }
        }

        public ObjectMapperData MapperData
            => _mapperData ?? (_mapperData = _mapper?.MapperData ?? ObjectMapperData.For<TSource, TTarget>(this));

        public TTarget CreatedObject { get; set; }

        #region IObjectMappingData Members

        public bool IsRoot => Parent == null;

        public IObjectMappingData Parent { get; }

        IObjectMappingDataUntyped IObjectMappingData<TSource, TTarget>.Parent => Parent;

        public bool IsPartOfDerivedTypeMapping => DeclaredTypeMappingData != null;

        public IObjectMappingData DeclaredTypeMappingData { get; }

        private ChildMemberMappingData<TSource, TTarget> _childMappingData;

        IChildMemberMappingData IObjectMappingData.GetChildMappingData(IMemberMapperData childMapperData)
        {
            if (_childMappingData == null)
            {
                _childMappingData = new ChildMemberMappingData<TSource, TTarget>(this);
            }

            _childMappingData.MapperData = childMapperData;

            return _childMappingData;
        }

        #endregion

        #region Map Methods

        object IObjectMappingData.MapStart() => MapStart();

        public TTarget MapStart() => _mapper.Map(this);

        public TDeclaredTarget Map<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            string targetMemberName,
            int dataSourceIndex)
        {
            var childMappingData = GetChildMappingData(
                sourceValue,
                targetValue,
                GetEnumerableIndex(),
                targetMemberName,
                dataSourceIndex);

            return (TDeclaredTarget)_mapper.MapSubObject(childMappingData);
        }

        private IObjectMappingData GetChildMappingData<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex)
        {
            return ObjectMappingDataFactory.ForChild(
                sourceValue,
                targetValue,
                enumerableIndex,
                targetMemberName,
                dataSourceIndex,
                this);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex)
        {
            var elementMappingData = ObjectMappingDataFactory.ForElement(
                sourceElement,
                targetElement,
                enumerableIndex,
                this);

            return (TTargetElement)_mapper.MapSubObject(elementMappingData);
        }

        TDeclaredTarget IObjectMappingDataUntyped.MapRecursion<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex)
        {
            if (IsRoot || MapperKey.MappingTypes.RuntimeTypesNeeded)
            {
                var childMappingData = GetChildMappingData(
                    sourceValue,
                    targetValue,
                    enumerableIndex,
                    targetMemberName,
                    dataSourceIndex);

                return (TDeclaredTarget)_mapper.MapRecursion(childMappingData);
            }

            return Parent.MapRecursion(
                sourceValue,
                targetValue,
                enumerableIndex,
                targetMemberName,
                dataSourceIndex);
        }

        #endregion

        public bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
            where TComplex : class
        {
            if (!IsRoot)
            {
                return Parent.TryGet(key, out complexType);
            }

            if (_mappedObjectsBySource.TryGetValue(key, out var mappedTargets))
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

            if (_mappedObjectsBySource.TryGetValue(key, out var mappedTargets))
            {
                mappedTargets.Add(complexType);
                return;
            }

            _mappedObjectsBySource[key] = new List<object> { complexType };
        }

        public IObjectMappingData<TNewSource, TTarget> WithSourceType<TNewSource>()
            where TNewSource : class
        {
            return As(Source as TNewSource, Target);
        }

        public IObjectMappingData<TSource, TNewTarget> WithTargetType<TNewTarget>()
            where TNewTarget : class
        {
            return As(Source, Target as TNewTarget);
        }

        public IObjectMappingData WithTypes(Type newSourceType, Type newTargetType)
        {
            var typesKey = new SourceAndTargetTypesKey(newSourceType, newTargetType);

            var typedWithTypesCaller = GlobalContext.Instance.Cache.GetOrAdd(typesKey, k =>
            {
                var mappingDataParameter = Parameters.Create<IObjectMappingData<TSource, TTarget>>("mappingData");
                var withTypesCall = mappingDataParameter.GetAsCall(k.SourceType, k.TargetType);

                var withTypesLambda = Expression
                    .Lambda<Func<IObjectMappingData<TSource, TTarget>, IObjectMappingDataUntyped>>(
                        withTypesCall,
                        mappingDataParameter);

                return withTypesLambda.Compile();
            });

            return (IObjectMappingData)typedWithTypesCaller.Invoke(this);
        }

        public IObjectMappingData<TNewSource, TNewTarget> As<TNewSource, TNewTarget>()
            where TNewSource : class
            where TNewTarget : class
        {
            return As(Source as TNewSource, Target as TNewTarget);
        }

        private IObjectMappingData<TNewSource, TNewTarget> As<TNewSource, TNewTarget>(
            TNewSource typedSource,
            TNewTarget typedTarget)
        {
            return new ObjectMappingData<TNewSource, TNewTarget>(
                typedSource,
                typedTarget,
                GetEnumerableIndex(),
                MapperKey.WithTypes<TNewSource, TNewTarget>(),
                MappingContext,
                this,
                Parent);
        }
    }
}