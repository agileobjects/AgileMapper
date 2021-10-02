namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface ITypedMapperKey : IMapperKeyDataOwner
    {
        MappingTypes MappingTypes { get; }

        IObjectMappingData CreateMappingData();

        bool Equals(ITypedMapperKey otherKey);
    }
}