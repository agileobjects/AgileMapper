namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal interface ITypedMapperKey : IMappingDataOwner
    {
        MappingTypes MappingTypes { get; }
    }
}