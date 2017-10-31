namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Caching;
    using ComplexTypes;
    using Enumerables;
    using Extensions;

    internal class ObjectMapperFactory
    {
        private readonly IList<MappingExpressionFactoryBase> _mappingExpressionFactories;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _rootMappersCache;

        public ObjectMapperFactory(MapperContext mapperContext)
        {
            _mappingExpressionFactories = new MappingExpressionFactoryBase[]
            {
                new DictionaryMappingExpressionFactory(),
                new SimpleTypeMappingExpressionFactory(),
                new EnumerableMappingExpressionFactory(),
                new ComplexTypeMappingExpressionFactory(mapperContext)
            };

            _rootMappersCache = mapperContext.Cache.CreateScoped<ObjectMapperKeyBase, IObjectMapper>();
        }

        public IEnumerable<IObjectMapper> RootMappers => _rootMappersCache.Values;

        public ObjectMapper<TSource, TTarget> GetOrCreateRoot<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            mappingData.MapperKey.MappingData = mappingData;

            var mapper = _rootMappersCache.GetOrAdd(
                mappingData.MapperKey,
                key =>
                {
                    var mapperToCache = key.MappingData.Mapper;

                    key.MappingData = null;

                    return mapperToCache;
                });

            return (ObjectMapper<TSource, TTarget>)mapper;
        }

        public ObjectMapper<TSource, TTarget> Create<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            var mappingExpressionFactory = _mappingExpressionFactories.First(mef => mef.IsFor(mappingData));
            var mappingExpression = mappingExpressionFactory.Create(mappingData);

            if (mappingExpression.NodeType == ExpressionType.Default)
            {
                return ObjectMapper<TSource, TTarget>.Unmappable;
            }

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

            _rootMappersCache.Empty();
        }
    }
}