namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal class ObjectMapperFactory
    {
        private readonly EnumerableMappingLambdaFactory _enumerableMappingLambdaFactory;
        private readonly ComplexTypeMappingLambdaFactory _complexTypeMappingLambdaFactory;

        public ObjectMapperFactory(MapperContext mapperContext)
        {
            _enumerableMappingLambdaFactory = new EnumerableMappingLambdaFactory();
            _complexTypeMappingLambdaFactory = new ComplexTypeMappingLambdaFactory(mapperContext);
        }

        public IObjectMapper<TTarget> CreateFor<TSource, TTarget>(IObjectMapperCreationData data)
        {
            var objectMapper = data.MapperData.MapperContext.Cache.GetOrAdd(data.MapperData.MapperKey, k =>
            {
                var lambda = data.TargetMember.IsEnumerable
                    ? _enumerableMappingLambdaFactory.Create<TSource, TTarget>(data)
                    : _complexTypeMappingLambdaFactory.Create<TSource, TTarget>(data);

                IObjectMapper<TTarget> mapper = new ObjectMapper<TSource, TTarget>(lambda);

                return mapper;
            });

            return objectMapper;
        }

        public void Reset()
        {
            _complexTypeMappingLambdaFactory.Reset();
        }
    }
}