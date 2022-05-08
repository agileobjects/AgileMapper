namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys;

internal interface IMapperKeyData : IMapperContextOwner, IRuleSetOwner
{
    MappingTypes MappingTypes { get; }

    object Source { get; }

    IObjectMappingData GetMappingData();
}