namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Caching;

    internal class ObjectMapperFactory
    {
        private readonly EnumerableMappingLambdaFactory _enumerableMappingLambdaFactory;
        private readonly ComplexTypeMappingLambdaFactory _complexTypeMappingLambdaFactory;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _rootMappers;

        public ObjectMapperFactory(MapperContext mapperContext)
        {
            _enumerableMappingLambdaFactory = new EnumerableMappingLambdaFactory();
            _complexTypeMappingLambdaFactory = new ComplexTypeMappingLambdaFactory(mapperContext);
            _rootMappers = mapperContext.Cache.CreateScoped<ObjectMapperKeyBase, IObjectMapper>();
        }

        public void CreateRoot(IObjectMappingData mappingData) => _rootMappers.GetOrAddMapper(mappingData);

        public IObjectMapper Create<TSource, TTarget>(IObjectMappingData mappingData)
        {
            var lambda = mappingData.MapperKey.MappingTypes.IsEnumerable
                ? _enumerableMappingLambdaFactory.Create<TSource, TTarget>(mappingData)
                : _complexTypeMappingLambdaFactory.Create<TSource, TTarget>(mappingData);

            var mapper = new ObjectMapper<TSource, TTarget>(lambda, mappingData.MapperData);

            return mapper;
        }

        public void Reset()
        {
            _rootMappers.Empty();
            _complexTypeMappingLambdaFactory.Reset();
        }
    }
}