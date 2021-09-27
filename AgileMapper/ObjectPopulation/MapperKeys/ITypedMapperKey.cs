namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface ITypedMapperKey : IMappingExecutionContextOwner
    {
        MappingTypes MappingTypes { get; }

        IObjectMappingData CreateMappingData();

        bool Equals(ITypedMapperKey otherKey);
    }
}