namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using Plans;

    internal abstract class SubObjectMappingExecutionContextBase<TSubSource> :
        MappingExecutionContextBase2<TSubSource>
    {
        private readonly IMappingExecutionContextInternal _parent;
        private readonly IEntryPointMappingContext _entryPointContext;

        protected SubObjectMappingExecutionContextBase(
            TSubSource source,
            IMappingExecutionContext parent,
            IEntryPointMappingContext entryPointContext)
            : base(source, parent)
        {
            _parent = (IMappingExecutionContextInternal)parent;
            _entryPointContext = entryPointContext;
        }

        public override MapperContext MapperContext => _entryPointContext.MapperContext;

        public override MappingRuleSet RuleSet => _entryPointContext.RuleSet;

        public override MappingPlanSettings PlanSettings => _entryPointContext.PlanSettings;

        public override MappingTypes MappingTypes => GetMapperKey().MappingTypes;

        public override IObjectMapper GetRootMapper() => _entryPointContext.GetRootMapper();

        protected IObjectMappingData GetParentMappingData() => _parent.ToMappingData();
    }
}