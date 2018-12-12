namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal struct RootMapperKeyFactory : IRootMapperKeyFactory
    {
        public ObjectMapperKeyBase CreateRootKeyFor(IObjectMappingData mappingData)
        {
            return new RootObjectMapperKey(mappingData.MappingTypes, mappingData.MappingContext)
            {
                MappingData = mappingData
            };
        }
    }
}