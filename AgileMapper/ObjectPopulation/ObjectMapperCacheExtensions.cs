namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Caching;

    internal static class ObjectMapperCacheExtensions
    {
        public static IObjectMapper GetOrAddMapper(
            this ICache<ObjectMapperKeyBase, IObjectMapper> objectMapperCache,
            IObjectMappingData mappingData)
        {
            var mapper = objectMapperCache.GetOrAdd(
                mappingData.MapperKey,
                key => key.MappingData.CreateMapper());

            return mapper;
        }
    }
}