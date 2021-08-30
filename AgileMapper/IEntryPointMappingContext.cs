namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;

    internal interface IEntryPointMappingContext : IMappingContext
    {
        MappingTypes MappingTypes { get; }

        TSource GetSource<TSource>();

        ObjectMapperKeyBase GetMapperKey();

        IObjectMappingData ToMappingData();

        IObjectMapper GetRootMapper();
    }
}