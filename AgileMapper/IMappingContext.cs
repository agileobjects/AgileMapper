namespace AgileObjects.AgileMapper
{
    internal interface IMappingContext
    {
        MapperContext MapperContext { get; }

        MappingRuleSet RuleSet { get; }

        bool AddUnsuccessfulMemberPopulations { get; }

        bool LazyLoadRecursionMappingFuncs { get; }
    }
}