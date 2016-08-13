namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Caching;

    internal class ObjectMapperFactory
    {
        private readonly ICache<ObjectMapperKey, object> _cache;

        public ObjectMapperFactory()
        {
            _cache = GlobalContext.Instance.CreateCache<ObjectMapperKey, object>();
        }

        public IObjectMapper<TTarget> CreateFor<TSource, TTarget>(IObjectMapperCreationData data)
        {
            var mapper = (IObjectMapper<TTarget>)_cache.GetOrAdd(data.MapperData.MapperKey, k =>
            {
                var lambda = data.TargetMember.IsEnumerable
                    ? EnumerableMappingLambdaFactory.Instance.Create<TSource, TTarget>(data)
                    : ComplexTypeMappingLambdaFactory.Instance.Create<TSource, TTarget>(data);

                return new ObjectMapper<TSource, TTarget>(lambda);
            });

            return mapper;
        }

        public void Reset()
        {
            _cache.Empty();
        }
    }
}