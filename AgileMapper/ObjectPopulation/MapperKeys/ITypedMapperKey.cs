namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface ITypedMapperKey : IMappingDataOwner
    {
        MappingTypes MappingTypes { get; }

        bool Equals(ITypedMapperKey otherKey);
    }
}