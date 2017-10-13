namespace AgileObjects.AgileMapper
{
    using System;
    using Api;
    using Api.Configuration;
    using Configuration;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class MappingExecutor<TSource> : ITargetTypeSelector<TSource>, IMappingContext
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

        public TResult ToANew<TResult>()
            => PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));

        public TResult ToANew<TResult>(Action<IFullMappingConfigurator<TSource, TResult>> configuration)
            => PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult), configuration);

        public TTarget OnTo<TTarget>(TTarget existing)
            => PerformMapping(MapperContext.RuleSets.Merge, existing);

        public TTarget Over<TTarget>(TTarget existing)
            => PerformMapping(MapperContext.RuleSets.Overwrite, existing);

        private TTarget PerformMapping<TTarget>(
            MappingRuleSet ruleSet,
            TTarget target,
            Action<IFullMappingConfigurator<TSource, TTarget>> configuration)
        {
            var mapperContext = new MapperContext();

            var configInfo = new MappingConfigInfo(mapperContext)
                .ForRuleSet(ruleSet)
                .ForSourceType<TSource>()
                .ForTargetType<TTarget>();

            configuration.Invoke(new MappingConfigurator<TSource, TTarget>(configInfo));

            return target;
        }

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
                new RootObjectMapperKey(MappingTypes<TSource, TTarget>.Fixed, this),
                this,
                parent: null);
        }

        private IObjectMappingData CreateRootMappingData<TTarget>(TTarget target)
            => CreateRootMappingData(_source, target);

        public IObjectMappingData CreateRootMappingData<TDataSource, TDataTarget>(TDataSource source, TDataTarget target)
            => ObjectMappingDataFactory.ForRoot(source, target, this);
    }
}