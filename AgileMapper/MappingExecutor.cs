namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
#if DYNAMIC_SUPPORTED
    using System.Dynamic;
#endif
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Extensions.Internal;
    using ObjectPopulation;

    internal class MappingExecutor<TSource> :
        ITargetSelector<TSource>,
        IFlatteningSelector<TSource>,
        IMappingContext
    {
        private readonly TSource _source;

        public MappingExecutor(TSource source, MapperContext mapperContext)
        {
            _source = source;
            MapperContext = mapperContext;
        }

        public MapperContext MapperContext { get; private set; }

        public MappingRuleSet RuleSet { get; private set; }

        public bool AddUnsuccessfulMemberPopulations => false;

        public bool LazyLoadRecursionMappingFuncs => true;

        #region ToANew Overloads

        public object ToANew(Type resultType) => MappingExecutorBridge<TSource>.CreateNew(resultType, this);

        public TResult ToANew<TResult>() => PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));

        public TResult ToANew<TResult>(Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult), new[] { configuration })
                : PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));
        }

        public TResult ToANew<TResult>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult), configurations)
                : PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));
        }

        #endregion

        #region OnTo Overloads

        public TTarget OnTo<TTarget>(TTarget existing) => PerformMapping(MapperContext.RuleSets.Merge, existing);

        public TTarget OnTo<TTarget>(TTarget existing, Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(MapperContext.RuleSets.Merge, existing, new[] { configuration })
                : PerformMapping(MapperContext.RuleSets.Merge, existing);
        }

        public TTarget OnTo<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(MapperContext.RuleSets.Merge, existing, configurations)
                : PerformMapping(MapperContext.RuleSets.Merge, existing);
        }

        #endregion

        #region Over Overloads

        public TTarget Over<TTarget>(TTarget existing) => PerformMapping(MapperContext.RuleSets.Overwrite, existing);

        public TTarget Over<TTarget>(TTarget existing, Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(MapperContext.RuleSets.Overwrite, existing, new[] { configuration })
                : PerformMapping(MapperContext.RuleSets.Overwrite, existing);
        }

        public TTarget Over<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(MapperContext.RuleSets.Overwrite, existing, configurations)
                : PerformMapping(MapperContext.RuleSets.Overwrite, existing);
        }

        #endregion

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget target)
        {
            if (_source == null)
            {
                return target;
            }

            RuleSet = ruleSet;

            return PerformMapping(target);
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

        private TTarget PerformMapping<TTarget>(TTarget target)
        {
            if (MappingTypes<TSource, TTarget>.SkipTypesCheck)
            {
                // Optimise for the most common scenario:
                var typedRootMappingData = ObjectMappingDataFactory
                    .ForRootFixedTypes(_source, target, this);

                return typedRootMappingData.MapStart();
            }

            var rootMappingData = ObjectMappingDataFactory.ForRoot(_source, target, this);
            var result = rootMappingData.MapStart();

            return (TTarget)result;
        }

        #region IFlatteningSelector Members

#if DYNAMIC_SUPPORTED
        dynamic IFlatteningSelector<TSource>.ToDynamic(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, ExpandoObject>>>[] configurations)
        {
            return configurations.Any() ? ToANew(configurations) : ToANew<ExpandoObject>();
        }
#endif

        Dictionary<string, object> IFlatteningSelector<TSource>.ToDictionary(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, object>>>>[] configurations)
        {
            return configurations.Any() ? ToANew(configurations) : ToANew<Dictionary<string, object>>();
        }

        Dictionary<string, TValue> IFlatteningSelector<TSource>.ToDictionary<TValue>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, TValue>>>>[] configurations)
        {
            return configurations.Any() ? ToANew(configurations) : ToANew<Dictionary<string, TValue>>();
        }

        string IFlatteningSelector<TSource>.ToQueryString(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, string>>>>[] configurations)
        {
            var flattened = configurations.Any() ? ToANew(configurations) : ToANew<Dictionary<string, string>>();

            var queryString = flattened
                .Project(kvp => Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value))
                .Join("&");
#if NET35
            queryString = queryString.Replace("!", "%21");
#endif
            return queryString.Replace(".", "%2E");
        }

        #endregion
    }
}