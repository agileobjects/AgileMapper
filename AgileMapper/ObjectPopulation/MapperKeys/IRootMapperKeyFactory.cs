namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface IRootMapperKeyFactory
    {
        ObjectMapperKeyBase CreateRootKeyFor(IObjectMappingData mappingData);
    }
}