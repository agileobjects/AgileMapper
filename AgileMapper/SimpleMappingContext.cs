namespace AgileObjects.AgileMapper
{
    using Plans;

    internal class SimpleMappingContext : IMappingContext
    {
        public SimpleMappingContext(MappingRuleSet ruleSet, MapperContext mapperContext)
            : this(ruleSet, MappingPlanSettings.Default.LazyPlanned, mapperContext)
        {
        }

        public SimpleMappingContext(
            MappingRuleSet ruleSet,
            MappingPlanSettings planSettings,
            MapperContext mapperContext)
        {
            MapperContext = mapperContext;
            RuleSet = ruleSet;
            PlanSettings = planSettings;
        }

        public MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; }

        public MappingPlanSettings PlanSettings { get; }
    }
}