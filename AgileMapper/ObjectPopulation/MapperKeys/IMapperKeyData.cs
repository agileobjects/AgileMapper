namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface IMapperKeyData
    {
        object Source { get; }

        IObjectMappingData GetMappingData();
    }
}