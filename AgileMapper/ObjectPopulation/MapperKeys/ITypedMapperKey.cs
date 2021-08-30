namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface ITypedMapperKey : IMappingDataOwner
    {
        MappingTypes MappingTypes { get; }

        IEntryPointMappingContext MappingContext { get; }

        IObjectMappingData CreateMappingData();

        bool Equals(ITypedMapperKey otherKey);
    }
}