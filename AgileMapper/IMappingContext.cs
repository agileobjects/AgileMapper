namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;

    internal interface IMappingContext
    {
        MapperContext MapperContext { get; }

        MappingRuleSet RuleSet { get; }

        IObjectMappingContextData CreateRootMappingContextData<TSource, TTarget>(TSource source, TTarget target);

        TTarget Map<TSource, TTarget>(IObjectMappingContextData data);
    }
}