namespace AgileObjects.AgileMapper
{
    internal class SimpleMappingContext : IMappingContext
    {
        public SimpleMappingContext(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            MapperContext = mapperContext;
            RuleSet = ruleSet;
            IgnoreUnsuccessfulMemberPopulations = true;
        }

        public MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; }

        public bool IncludeCodeComments { get; set; }

        public bool IgnoreUnsuccessfulMemberPopulations { get; set; }

        public bool LazyLoadRepeatMappingFuncs => false;
    }
}