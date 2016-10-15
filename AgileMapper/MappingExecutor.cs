namespace AgileObjects.AgileMapper
{
    using Api;
    using ObjectPopulation;

    internal class MappingExecutor<TSource> : ITargetTypeSelector, IMappingContext
    {
        private readonly TSource _source;

        public MappingExecutor(TSource source, MapperContext mapperContext)
        {
            _source = source;
            MapperContext = mapperContext;
        }

        public MappingExecutor(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            RuleSet = ruleSet;
            MapperContext = mapperContext;
        }

        public MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; private set; }

        public TResult ToANew<TResult>() where TResult : class
            => PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));

        public TTarget OnTo<TTarget>(TTarget existing) where TTarget : class
            => PerformMapping(MapperContext.RuleSets.Merge, existing);

        public TTarget Over<TTarget>(TTarget existing) where TTarget : class
            => PerformMapping(MapperContext.RuleSets.Overwrite, existing);

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget existing)
        {
            if (_source == null)
            {
                return existing;
            }

            RuleSet = ruleSet;

            var rootMappingData = CreateRootMappingData(existing);
            var result = rootMappingData.MapStart();

            return (TTarget)result;
        }

        private IObjectMappingData CreateRootMappingData<TTarget>(TTarget target)
            => CreateRootMappingData(_source, target);

        public IObjectMappingData CreateRootMappingData<TDataSource, TDataTarget>(TDataSource source, TDataTarget target)
            => ObjectMappingDataFactory.ForRoot(source, target, this);
    }
}