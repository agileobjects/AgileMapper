namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;
    using Plans;

    internal interface IMappingContext : IMapperContextOwner, IRuleSetOwner
    {
        MappingPlanSettings PlanSettings { get; }
    }

    internal interface IEntryPointMappingContext : IMappingContext
    {
        MappingTypes MappingTypes { get; }

        IRootMapperKey GetRootMapperKey();

        IObjectMappingData ToMappingData();

        TSource GetSource<TSource>();
    }
}