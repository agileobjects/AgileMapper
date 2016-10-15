namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Caching;

    internal static class ObjectMapperCacheExtensions
    {
        public static IObjectMapper GetOrAddMapper(
            this ICache<ObjectMapperKeyBase, IObjectMapper> objectMapperCache,
            IObjectMappingData mappingData)
        {
            mappingData.Mapper = objectMapperCache.GetOrAdd(
                mappingData.MapperKey,
                key => key.MappingData.CreateMapper());

            return mappingData.Mapper;
        }
    }
}