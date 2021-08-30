namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;
    using Plans;

    internal class ChildMappingExecutionContext<TChildSource, TChildTarget> :
        MappingExecutionContextBase2<TChildSource>
    {
        private readonly IEntryPointMappingContext _parentContext;
        private readonly TChildSource _source;
        private readonly TChildTarget _target;
        private readonly int? _elementIndex;
        private readonly object _elementKey;
        private readonly ObjectMapperKeyBase _mapperKey;

        public ChildMappingExecutionContext(
            TChildSource source,
            TChildTarget target,
            int? elementIndex,
            object elementKey,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IMappingExecutionContext parent,
            IEntryPointMappingContext parentContext)
            : base(source, parent)
        {
            _source = source;
            _target = target;
            _elementIndex = elementIndex;
            _elementKey = elementKey;
            _parentContext = parentContext;

            // TODO: Can this be an interface implemented by this class...?
            _mapperKey = new ChildObjectMapperKey(
                MappingTypes.For(source, target),
                targetMemberRegistrationName,
                dataSourceIndex)
            {
                MappingContext = this
            };
        }

        #region IMappingContext Members

        public override MapperContext MapperContext => _parentContext.MapperContext;

        public override MappingRuleSet RuleSet => _parentContext.RuleSet;

        public override MappingPlanSettings PlanSettings => _parentContext.PlanSettings;

        #endregion

        #region IEntryPointMappingContext Members

        public override MappingTypes MappingTypes => _mapperKey.MappingTypes;

        public override ObjectMapperKeyBase GetMapperKey() => _mapperKey;

        public override IObjectMappingData ToMappingData()
        {
            return ObjectMappingDataFactory.Create(
                _source,
                _target,
                _elementIndex,
                _elementKey,
                _mapperKey,
                _parentContext.ToMappingData());
        }

        public override IObjectMapper GetRootMapper() => _parentContext.GetRootMapper();

        #endregion
    }
}