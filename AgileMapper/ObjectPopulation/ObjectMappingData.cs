namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ObjectMappingData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IObjectMappingData,
        IObjectCreationMappingData<TSource, TTarget, TTarget>
    {
        private readonly IObjectMappingData _parent;
        private readonly Dictionary<int, Dictionary<object, object>> _mappedObjectsByTypes;
        private IObjectMapper _mapper;
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
                _parent = parent;
                return;
            }

            _mappedObjectsByTypes = new Dictionary<int, Dictionary<object, object>>();
            IsRoot = true;

            if (IsPartOfDerivedTypeMapping)
            {
                return;
            }

            Mapper = MapperContext.ObjectMapperFactory.GetOrCreateRoot(this);
        }

        public IMappingContext MappingContext { get; }

        public MapperContext MapperContext => MappingContext.MapperContext;

        public ObjectMapperKeyBase MapperKey { get; }

        public IObjectMapper Mapper
        {
            get { return _mapper ?? (Mapper = MapperContext.ObjectMapperFactory.Create<TSource, TTarget>(this)); }
            set
            {
                _mapper = value;
                _mapperData = _mapper.MapperData;
            }
        }

        public TTarget CreatedObject { get; set; }

        #region IObjectMappingData Members

        public bool IsRoot { get; }

        IObjectMappingData IObjectMappingData.Parent => _parent;

        public bool IsPartOfDerivedTypeMapping => DeclaredTypeMappingData != null;

        public IObjectMappingData DeclaredTypeMappingData { get; }

        public ObjectMapperData MapperData
            => _mapperData ?? (_mapperData = ObjectMapperData.For<TSource, TTarget>(this));

        private ChildMemberMappingData<TSource, TTarget> _childMappingData;

        IMemberMappingData IObjectMappingData.GetChildMappingData(IMemberMapperData childMapperData)
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

        public object MapStart() => Mapper.Map(this);

        public TDeclaredTarget Map<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            string targetMemberName,
            int dataSourceIndex)
        {
            return (TDeclaredTarget)Mapper.MapChild(
                sourceValue,
                targetValue,
                GetEnumerableIndex(),
                targetMemberName,
                dataSourceIndex,
                this);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex)
        {
            return (TTargetElement)Mapper.MapElement(
                sourceElement,
                targetElement,
                enumerableIndex,
                this);
        }

        public TDeclaredTarget MapRecursion<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            string targetMemberName,
            int dataSourceIndex)
        {
            if (IsRoot || MapperKey.MappingTypes.RuntimeTypesNeeded)
            {
                return (TDeclaredTarget)Mapper.MapRecursion(
                    sourceValue,
                    targetValue,
                    GetEnumerableIndex(),
                    targetMemberName,
                    dataSourceIndex,
                    this);
            }

            return _parent.MapRecursion(
                sourceValue,
                targetValue,
                targetMemberName,
                dataSourceIndex);
        }

        #endregion

        public bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
        {
            if (!IsRoot)
            {
                return _parent.TryGet(key, out complexType);
            }

            if (key != null)
            {
                var typesKey = SourceAndTargetTypeKey<TKey, TComplex>.Instance.GetHashCode();
                Dictionary<object, object> mappedTargetsBySource;

                if (_mappedObjectsByTypes.TryGetValue(typesKey, out mappedTargetsBySource))
                {
                    object mappedObject;

                    if (mappedTargetsBySource.TryGetValue(key, out mappedObject))
                    {
                        complexType = (TComplex)mappedObject;
                        return true;
                    }
                }
            }

            complexType = default(TComplex);
            return false;
        }

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
        {
            if (!IsRoot)
            {
                _parent.Register(key, complexType);
                return;
            }

            var typesKey = SourceAndTargetTypeKey<TKey, TComplex>.Instance.GetHashCode();
            Dictionary<object, object> mappedTargetsBySource;

            if (_mappedObjectsByTypes.TryGetValue(typesKey, out mappedTargetsBySource))
            {
                mappedTargetsBySource[key] = complexType;
                return;
            }

            _mappedObjectsByTypes[typesKey] = new Dictionary<object, object> { [key] = complexType };
        }

        private class SourceAndTargetTypeKey<TKey, TComplex>
        {
            public static readonly object Instance = new SourceAndTargetTypeKey<TKey, TComplex>();
        }

        public IObjectMappingData WithTypes(Type newSourceType, Type newTargetType)
        {
            var typesKey = new SourceAndTargetTypesKey(newSourceType, newTargetType);

            var typedWithTypesCaller = GlobalContext.Instance.Cache.GetOrAdd(typesKey, k =>
            {
                var mappingDataParameter = Parameters.Create<ObjectMappingData<TSource, TTarget>>("mappingData");

                var withTypesMethod = mappingDataParameter.Type
                    .GetNonPublicInstanceMethods()
                    .First(m => (m.Name == "WithTypes") && m.IsGenericMethod)
                    .MakeGenericMethod(k.SourceType, k.TargetType);

                var withTypesCall = Expression.Call(mappingDataParameter, withTypesMethod);

                var withTypesLambda = Expression
                    .Lambda<Func<ObjectMappingData<TSource, TTarget>, IObjectMappingData>>(
                        withTypesCall,
                        mappingDataParameter);

                return withTypesLambda.Compile();
            });

            return typedWithTypesCaller.Invoke(this);
        }

        // ReSharper disable once UnusedMember.Local
        private IObjectMappingData WithTypes<TNewSource, TNewTarget>()
            where TNewSource : class
            where TNewTarget : class
        {
            return new ObjectMappingData<TNewSource, TNewTarget>(
                Source as TNewSource,
                Target as TNewTarget,
                GetEnumerableIndex(),
                MapperKey.WithTypes<TNewSource, TNewTarget>(),
                MappingContext,
                this,
                _parent);
        }
    }
}