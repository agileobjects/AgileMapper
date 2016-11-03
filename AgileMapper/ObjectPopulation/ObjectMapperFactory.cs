namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;

    internal class ObjectMapperFactory
    {
        private readonly EnumerableMappingExpressionFactory _enumerableMappingExpressionFactory;
        private readonly ComplexTypeMappingExpressionFactory _complexTypeMappingExpressionFactory;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _rootMappers;

        public ObjectMapperFactory(MapperContext mapperContext)
        {
            _enumerableMappingExpressionFactory = new EnumerableMappingExpressionFactory();
            _complexTypeMappingExpressionFactory = new ComplexTypeMappingExpressionFactory(mapperContext);
            _rootMappers = mapperContext.Cache.CreateScoped<ObjectMapperKeyBase, IObjectMapper>();
        }

        public IObjectMapper GetOrCreateRoot(IObjectMappingData mappingData) => _rootMappers.GetOrAddMapper(mappingData);

        public IObjectMapper Create<TSource, TTarget>(IObjectMappingData mappingData)
        {
            var mappingExpression = mappingData.MapperKey.MappingTypes.IsEnumerable
                ? _enumerableMappingExpressionFactory.Create(mappingData)
                : _complexTypeMappingExpressionFactory.Create(mappingData);

            var mappingLambda = Expression.Lambda<MapperFunc<TSource, TTarget>>(
                mappingExpression,
                mappingData.MapperData.MappingDataObject);

            var mapper = new ObjectMapper<TSource, TTarget>(mappingLambda, mappingData.MapperData);

            return mapper;
        }

        public void Reset()
        {
            _rootMappers.Empty();
            _complexTypeMappingExpressionFactory.Reset();
        }
    }
}