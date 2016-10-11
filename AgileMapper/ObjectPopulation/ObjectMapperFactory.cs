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

        public IObjectMapper<TTarget> CreateFor<TSource, TTarget>(IObjectMappingContextData data)
        {
            var objectMapper = data.MapperData.MapperContext.Cache.GetOrAdd(
                (IObjectMapperKey)data,
                key =>
                {
                    var contextData = (IObjectMappingContextData)key;

                    var lambda = contextData.TargetMember.IsEnumerable
                        ? _enumerableMappingLambdaFactory.Create<TSource, TTarget>(contextData)
                        : _complexTypeMappingLambdaFactory.Create<TSource, TTarget>(contextData);

                    IObjectMapper<TTarget> mapper = new ObjectMapper<TSource, TTarget>(lambda);

                    return mapper;
                },
                key => ((IObjectMappingContextData)key).MapperKeyObject);

            return objectMapper;
        }

        public void Reset()
        {
            _complexTypeMappingLambdaFactory.Reset();
        }
    }
}