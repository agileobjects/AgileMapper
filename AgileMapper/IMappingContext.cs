namespace AgileObjects.AgileMapper
{
    internal interface IMappingContext
    {
        MapperContext MapperContext { get; }

        MappingRuleSet RuleSet { get; }
    }
}