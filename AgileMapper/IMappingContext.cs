namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;

    internal interface IMappingContext
    {
        MapperContext MapperContext { get; }

        MappingRuleSet RuleSet { get; }

        IObjectMappingData CreateRootMappingData<TSource, TTarget>(TSource source, TTarget target);
    }
}