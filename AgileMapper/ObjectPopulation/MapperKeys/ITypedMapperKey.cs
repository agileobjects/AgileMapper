namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface ITypedMapperKey : IMappingDataOwner
    {
        MappingTypes MappingTypes { get; }

        MappingExecutionContextBase2 MappingContext { get; }

        IObjectMappingData CreateMappingData();

        bool Equals(ITypedMapperKey otherKey);
    }
}