namespace AgileObjects.AgileMapper
{
    using Api;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class MappingExecutor<TSource> : ITargetTypeSelector, IMappingContext
    {
        private static readonly bool _runtimeSourceTypeNeeded = TypeInfo<TSource>.RuntimeTypeNeeded;

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

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget target)
        {
            if (_source == null)
            {
                return target;
            }

            RuleSet = ruleSet;

            if (SkipTypeChecks<TTarget>())
            {
                // Optimise for the most common scenario:
                var typedRootMappingData = CreateTypedRootMappingData(target);

                return typedRootMappingData.MapStart();
            }

            var rootMappingData = CreateRootMappingData(target);
            var result = rootMappingData.MapStart();

            return (TTarget)result;
        }

        private static bool SkipTypeChecks<TTarget>()
            => !(_runtimeSourceTypeNeeded || TypeInfo<TTarget>.RuntimeTypeNeeded);

        private ObjectMappingData<TSource, TTarget> CreateTypedRootMappingData<TTarget>(TTarget target)
        {
            return new ObjectMappingData<TSource, TTarget>(
                _source,
                target,
                null, // <- No enumerable index because we're at the root
                new RootObjectMapperKey(MappingTypes.Fixed<TSource, TTarget>(), this),
                this,
                parent: null);
        }

        private IObjectMappingData CreateRootMappingData<TTarget>(TTarget target)
            => CreateRootMappingData(_source, target);

        public IObjectMappingData CreateRootMappingData<TDataSource, TDataTarget>(TDataSource source, TDataTarget target)
            => ObjectMappingDataFactory.ForRoot(source, target, this);
    }
}