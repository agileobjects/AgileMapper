namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal interface IRootMapperKey : ITypedMapperKey
    {
        MappingRuleSet RuleSet { get; }
    }
}