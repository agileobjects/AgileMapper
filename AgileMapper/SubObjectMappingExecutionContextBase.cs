namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using Plans;

    internal abstract class SubObjectMappingExecutionContextBase<TSubSource> :
        MappingExecutionContextBase2<TSubSource>
    {
        private readonly MappingExecutionContextBase2 _parent;
        private readonly MappingExecutionContextBase2 _entryPointContext;

        protected SubObjectMappingExecutionContextBase(
            TSubSource source,
            MappingExecutionContextBase2 parent,
            MappingExecutionContextBase2 entryPointContext)
            : base(source, parent)
        {
            _parent = parent;
            _entryPointContext = entryPointContext;
        }

        public override MapperContext MapperContext => _entryPointContext.MapperContext;

        public override MappingRuleSet RuleSet => _entryPointContext.RuleSet;

        public override MappingPlanSettings PlanSettings => _entryPointContext.PlanSettings;

        public override MappingTypes MappingTypes => GetMapperKey().MappingTypes;

        public override IObjectMapper GetRootMapper() => _entryPointContext.GetRootMapper();

        protected IObjectMappingData GetParentMappingData() => _parent.GetMappingData();
    }
}