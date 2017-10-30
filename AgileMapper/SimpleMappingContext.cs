namespace AgileObjects.AgileMapper
{
    internal class SimpleMappingContext : IMappingContext
    {
        public SimpleMappingContext(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            RuleSet = ruleSet;
            MapperContext = mapperContext;
        }


        public MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; }
    }
}