namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal static class DefaultRootMapperKeyFactory
    {
        public static ObjectMapperKeyBase Create(IObjectMappingData mappingData)
        {
            return new RootObjectMapperKey(mappingData.MappingTypes, mappingData.MappingContext)
            {
                MappingData = mappingData
            };
        }
    }
}