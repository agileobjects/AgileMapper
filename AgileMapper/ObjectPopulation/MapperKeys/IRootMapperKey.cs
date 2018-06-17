namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface IRootMapperKey : ITypedMapperKey
    {
        MappingRuleSet RuleSet { get; }

        bool Equals(IRootMapperKey otherKey);
    }
}