namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Caching;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper, IRepeatedMappingFuncSet
    {
        public static readonly ObjectMapper<TSource, TTarget> Unmappable = new ObjectMapper<TSource, TTarget>();

        private readonly MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _subMappersByKey;
        private readonly ICache<MappingTypes, IObjectMapperFunc> _mapperFuncsCache;
        private readonly bool _mappedObjectCacheRequired;
        private Action _resetCallback;

        private ObjectMapper()
        {
        }

        public ObjectMapper(
            Expression<MapperFunc<TSource, TTarget>> mappingLambda,
            IObjectMappingData mappingData)
        {
            MappingLambda = mappingLambda;
            MapperData = mappingData.MapperData;

            if (MapperData.Context.Compile)
            {
                _mapperFunc = mappingLambda.Compile();

                if (MappedObjectCacheRequired(MapperData))
                {
                    _mappedObjectCacheRequired = true;
                }
            }
            else if (MapperData.Context.NeedsSubMapping)
            {
                MapperData.Mapper = this;
            }

            if (MapperData.Context.NeedsSubMapping)
            {
                _subMappersByKey = MapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }

            if (MapperData.HasMapperFuncs)
            {
                _mapperFuncsCache = CreateRepeatedMappingFuncsCache(mappingData);
            }
        }

        #region Setup

        private static bool MappedObjectCacheRequired(ObjectMapperData mapperData)
        {
            if (mapperData.CacheMappedObjects)
            {
                return true;
            }

            return mapperData.ChildMapperDatas.Any(MappedObjectCacheRequired);
        }

        private ICache<MappingTypes, IObjectMapperFunc> CreateRepeatedMappingFuncsCache(
            IObjectMappingData mappingData)
        {
            var cache = MapperData.MapperContext.Cache.CreateNew<MappingTypes, IObjectMapperFunc>();

            // The iteration requires a for loop because items can get added 
            // to RequiredMapperFuncKeys by virtue of creating the Mapper:
            for (var i = 0; i < MapperData.RequiredMapperFuncKeys.Count; ++i)
            {
                var mapperKey = MapperData.RequiredMapperFuncKeys[i];

                var mappingTypes = new MappingTypes(
                    mapperKey.MapperData.SourceType,
                    mapperKey.MapperData.TargetType);

                var mappingFuncCreator = GlobalContext.Instance.Cache.GetOrAdd(mappingTypes, key =>
                {
                    var mapperFuncType = typeof(RepeatedMappingFunc<,>).MakeGenericType(key.SourceType, key.TargetType);
                    var lazyLoadParameter = Parameters.Create<bool>("lazyLoadFuncs");

                    var mapperFuncConstructor = mapperFuncType.GetPublicInstanceConstructor(
                        typeof(IRepeatedMappingFuncSet),
                        typeof(IObjectMappingData),
                        typeof(bool));

                    var mapperFuncCreation = Expression.New(
                        mapperFuncConstructor,
                        Parameters.RepeatedMappingFuncs,
                        Parameters.ObjectMappingData,
                        lazyLoadParameter);

                    var mappingFuncCreationLambda = Expression
                        .Lambda<Func<IRepeatedMappingFuncSet, IObjectMappingData, bool, IObjectMapperFunc>>(
                            mapperFuncCreation,
                            Parameters.RepeatedMappingFuncs,
                            Parameters.ObjectMappingData,
                            lazyLoadParameter);

                    return mappingFuncCreationLambda.Compile();
                });

                var mapperFunc = mappingFuncCreator.Invoke(
                    this,
                    mapperKey.MappingData,
                    mappingData.MappingContext.LazyLoadRepeatedMappingFuncs);

                cache.GetOrAdd(mappingTypes, k => mapperFunc);
            }

            return cache;
        }

        #endregion

        public Type SourceType => typeof(TSource);

        public Type TargetType => typeof(TTarget);

        public LambdaExpression MappingLambda { get; }

        public bool IsNullObject => this == Unmappable;

        public Expression MappingExpression => MappingLambda.Body;

        public ObjectMapperData MapperData { get; }

        public IEnumerable<IObjectMapperFunc> RepeatedMappingFuncs => _mapperFuncsCache.Values;

        public bool IsStaticallyCacheable(ObjectMapperKeyBase mapperKey)
        {
            if (mapperKey.HasTypeTester)
            {
                return false;
            }

            if (_subMappersByKey == null)
            {
                return true;
            }

            for (var i = 0; i < _subMappersByKey.Count; i++)
            {
                var subMapperByKey = _subMappersByKey[i];

                if (!subMapperByKey.Value.IsStaticallyCacheable(subMapperByKey.Key))
                {
                    return false;
                }
            }

            return true;
        }

        public object Map(IObjectMappingData mappingData) => Map((ObjectMappingData<TSource, TTarget>)mappingData);

        public TTarget Map(ObjectMappingData<TSource, TTarget> mappingData)
        {
            return _mapperFunc.Invoke(
                mappingData,
                _mappedObjectCacheRequired ? new ObjectCache() : null,
                this);
        }

        public TChildTarget Map<TChildSource, TChildTarget>(
            IObjectMappingData<TChildSource, TChildTarget> mappingData,
            ObjectCache mappedObjectsCache)
        {
            var mapperFunc = (RepeatedMappingFunc<TChildSource, TChildTarget>)_mapperFuncsCache
                .GetOrAdd(MappingTypes<TChildSource, TChildTarget>.Fixed, null);

            return mapperFunc.Map(
                (ObjectMappingData<TChildSource, TChildTarget>)mappingData,
                mappedObjectsCache);
        }

        public object MapRuntimeTypedSubObject(IObjectMappingData mappingData)
        {
            mappingData.MapperKey.MappingData = mappingData;

            var mapper = _subMappersByKey.GetOrAdd(
                mappingData.MapperKey,
                key =>
                {
                    var mapperToCache = key.MappingData.Mapper;

                    key.MappingData = null;

                    return mapperToCache;
                });

            return mapper.Map(mappingData);
        }

        public ObjectMapper<TSource, TTarget> WithResetCallback(Action callback)
        {
            _resetCallback = callback;
            return this;
        }

        public void Reset() => _resetCallback?.Invoke();
    }
}