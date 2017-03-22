namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;
    using ComplexTypes;
    using Enumerables;

    internal class ObjectMapperFactory
    {
        private readonly IEnumerable<MappingExpressionFactoryBase> _mappingExpressionFactories;
        private readonly List<ICacheEmptier> _rootCacheEmptiers;

        public ObjectMapperFactory(MapperContext mapperContext)
        {
            _mappingExpressionFactories = new MappingExpressionFactoryBase[]
            {
                new DictionaryMappingExpressionFactory(),
                new SimpleTypeMappingExpressionFactory(),
                new EnumerableMappingExpressionFactory(),
                new ComplexTypeMappingExpressionFactory(mapperContext)
            };

            _rootCacheEmptiers = new List<ICacheEmptier>();
        }

        public ObjectMapper<TSource, TTarget> GetOrCreateRoot<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            mappingData.MapperKey.MappingData = mappingData;

            var mapper = RootMapperCache<TSource, TTarget>.Mappers.GetOrAdd(
                mappingData.MapperKey,
                key =>
                {
                    var mapperToCache = (ObjectMapper<TSource, TTarget>)key.MappingData.Mapper;

                    key.MappingData = null;
                    _rootCacheEmptiers.Add(CacheEmptier<TSource, TTarget>.Instance);

                    return mapperToCache;
                });

            return mapper;
        }

        public ObjectMapper<TSource, TTarget> Create<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            var mappingExpressionFactory = _mappingExpressionFactories.First(mef => mef.IsFor(mappingData));
            var mappingExpression = mappingExpressionFactory.Create(mappingData);

            mappingExpression = MappingFactory
                .UseLocalSourceValueVariableIfAppropriate(mappingExpression, mappingData.MapperData);

            var mappingLambda = Expression.Lambda<MapperFunc<TSource, TTarget>>(
                mappingExpression,
                mappingData.MapperData.MappingDataObject);

            var mapper = new ObjectMapper<TSource, TTarget>(mappingLambda, mappingData.MapperData);

            return mapper;
        }

        public void Reset()
        {
            foreach (var mappingExpressionFactory in _mappingExpressionFactories)
            {
                mappingExpressionFactory.Reset();
            }

            foreach (var rootCacheEmptier in _rootCacheEmptiers)
            {
                rootCacheEmptier.EmptyCache();
            }

            _rootCacheEmptiers.Clear();
        }

        #region Root Mapper Caching

        private static class RootMapperCache<TSource, TTarget>
        {
            public static readonly ICache<ObjectMapperKeyBase, ObjectMapper<TSource, TTarget>> Mappers;

            static RootMapperCache()
            {
                Mappers = GlobalContext.Instance
                    .Cache
                    .CreateScoped<ObjectMapperKeyBase, ObjectMapper<TSource, TTarget>>();
            }
        }

        private interface ICacheEmptier
        {
            void EmptyCache();
        }

        private class CacheEmptier<TSource, TTarget> : ICacheEmptier
        {
            public static readonly ICacheEmptier Instance = new CacheEmptier<TSource, TTarget>();

            public void EmptyCache()
            {
                RootMapperCache<TSource, TTarget>.Mappers.Empty();
            }
        }

        #endregion
    }
}