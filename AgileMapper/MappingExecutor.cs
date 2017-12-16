namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Extensions.Internal;
    using ObjectPopulation;

    internal class MappingExecutor<TSource> : ITargetTypeSelector<TSource>, IMappingContext
    {
        private readonly TSource _source;

        public MappingExecutor(TSource source, MapperContext mapperContext)
        {
            _source = source;
            MapperContext = mapperContext;
        }

        public MapperContext MapperContext { get; private set; }

        public MappingRuleSet RuleSet { get; private set; }

        #region Inline Configuration

        public TResult ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult), configurations);
        }

        public TTarget OnTo<TTarget>(
            TTarget existing,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return PerformMapping(MapperContext.RuleSets.Merge, existing, configurations);
        }

        public TTarget Over<TTarget>(
            TTarget existing,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return PerformMapping(MapperContext.RuleSets.Overwrite, existing, configurations);
        }

        private TTarget PerformMapping<TTarget>(
            MappingRuleSet ruleSet,
            TTarget target,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            if (_source == null)
            {
                return target;
            }

            RuleSet = ruleSet;
            MapperContext = MapperContext.InlineContexts.GetContextFor(configurations, this);

            return PerformMapping(target);
        }

        #endregion

        public TResult ToANew<TResult>()
            => PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));

        public TTarget OnTo<TTarget>(TTarget existing)
            => PerformMapping(MapperContext.RuleSets.Merge, existing);

        public TTarget Over<TTarget>(TTarget existing)
            => PerformMapping(MapperContext.RuleSets.Overwrite, existing);

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget target)
        {
            if (_source == null)
            {
                return target;
            }

            RuleSet = ruleSet;

            return PerformMapping(target);
        }

        private TTarget PerformMapping<TTarget>(TTarget target)
        {
            if (TypeInfo<TSource>.RuntimeTypeNeeded || TypeInfo<TTarget>.RuntimeTypeNeeded)
            {
                var rootMappingData = ObjectMappingDataFactory.ForRoot(_source, target, this);
                var result = rootMappingData.MapStart();

                return (TTarget)result;
            }

            // Optimise for the most common scenario:
            var typedRootMappingData = ObjectMappingDataFactory
                .ForRootFixedTypes(_source, target, this);

            return typedRootMappingData.MapStart();
        }
    }
}