namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
#if FEATURE_DYNAMIC
    using System.Dynamic;
#endif
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Extensions;
    using Extensions.Internal;
    using ObjectPopulation;

    /// <summary>
    /// Provides options to execute a particular mapping from an object of the
    /// <typeparamref name="TSource"/> type.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of sources object from which this <see cref="MappingExecutor{TSource}"/> will
    /// perform mappings.
    /// </typeparam>
    public class MappingExecutor<TSource> :
        ITargetSelector<TSource>,
        IFlatteningSelector<TSource>,
        IUnflatteningSelector<TSource>,
        IMappingContext
    {
        private MapperContext _mapperContext;
        private readonly TSource _source;
        private MappingRuleSet _ruleSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingExecutor{TSource}"/> class with the
        /// given <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source object from which the mapping is to be performed.</param>
        protected internal MappingExecutor(TSource source)
            : this(Mapper.Default.Context, source)
        {
        }

        internal MappingExecutor(MapperContext mapperContext, TSource source)
        {
            _mapperContext = mapperContext.ThrowIfDisposed();
            _source = source;
        }

        MapperContext IMapperContextOwner.MapperContext => _mapperContext;

        MappingRuleSet IRuleSetOwner.RuleSet => _ruleSet;

        bool IMappingContext.IncludeCodeComments => false;

        bool IMappingContext.IgnoreUnsuccessfulMemberPopulations => true;

        bool IMappingContext.LazyLoadRepeatMappingFuncs => true;

        #region ToANew Overloads

        object ITargetSelector<TSource>.ToANew(Type resultType) => ToANew(resultType);

        private object ToANew(Type resultType) => MappingExecutorBridge<TSource>.CreateNew(resultType, this);

        TResult ITargetSelector<TSource>.ToANew<TResult>() => ToANew<TResult>();

        private TResult ToANew<TResult>() => PerformMapping(CreateNew, default(TResult));

        TResult ITargetSelector<TSource>.ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(CreateNew, default(TResult), new[] { configuration })
                : PerformMapping(CreateNew, default(TResult));
        }

        TResult ITargetSelector<TSource>.ToANew<TResult>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return ToANew(configurations);
        }

        private TResult ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(CreateNew, default(TResult), configurations)
                : PerformMapping(CreateNew, default(TResult));
        }

        private MappingRuleSet CreateNew => _mapperContext.RuleSets.CreateNew;

        #endregion

        #region OnTo Overloads

        TTarget ITargetSelector<TSource>.OnTo<TTarget>(TTarget existing)
            => PerformMapping(Merge, existing);

        TTarget ITargetSelector<TSource>.OnTo<TTarget>(
            TTarget existing,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(Merge, existing, new[] { configuration })
                : PerformMapping(Merge, existing);
        }

        TTarget ITargetSelector<TSource>.OnTo<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(Merge, existing, configurations)
                : PerformMapping(Merge, existing);
        }

        private MappingRuleSet Merge => _mapperContext.RuleSets.Merge;

        #endregion

        #region Over Overloads

        TTarget ITargetSelector<TSource>.Over<TTarget>(TTarget existing)
            => PerformMapping(Overwrite, existing);

        TTarget ITargetSelector<TSource>.Over<TTarget>(
            TTarget existing,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(Overwrite, existing, new[] { configuration })
                : PerformMapping(Overwrite, existing);
        }

        TTarget ITargetSelector<TSource>.Over<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(Overwrite, existing, configurations)
                : PerformMapping(Overwrite, existing);
        }

        private MappingRuleSet Overwrite => _mapperContext.RuleSets.Overwrite;

        #endregion

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget target)
        {
            if (_source == null)
            {
                return target;
            }

            _ruleSet = ruleSet;
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

            _ruleSet = ruleSet;
            _mapperContext = _mapperContext.InlineContexts.GetContextFor(configurations, this);
            return PerformMapping(target);
        }

        private TTarget PerformMapping<TTarget>(TTarget target)
        {
            if (MappingTypes<TSource, TTarget>.SkipTypesCheck)
            {
                // Optimise for the most common scenario:
                return PerformFixedTypesMapping(_source, target);
            }

            var rootMappingData = ObjectMappingDataFactory.ForRoot(_source, target, this);
            var result = rootMappingData.MapStart();

            return (TTarget)result;
        }

        /// <summary>
        /// Maps the given <paramref name="source"/> object to the given <paramref name="target"/>,
        /// without first checking the runtime Types of either object.
        /// </summary>
        /// <typeparam name="TTarget">The Type of object to which the mapping will be performed.</typeparam>
        /// <param name="source">The object from which the mapping will be performed.</param>
        /// <param name="target">The object to which the mapping will be performed, if applicable.</param>
        /// <returns>
        /// The given <paramref name="target"/> object, or a new, mapped <typeparamref name="TTarget"/>
        /// instance, as applicable.
        /// </returns>
        protected virtual TTarget PerformFixedTypesMapping<TTarget>(TSource source, TTarget target)
        {
            var typedRootMappingData = ObjectMappingDataFactory
                .ForRootFixedTypes(source, target, this, createMapper: true);

            return typedRootMappingData.MapStart();
        }

        #region IFlatteningSelector Members

#if FEATURE_DYNAMIC
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

        #region IUnflatteningSelector Members

        object IUnflatteningSelector<TSource>.To(Type resultType) => ToANew(resultType);

        TResult IUnflatteningSelector<TSource>.To<TResult>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return configurations.Any() ? ToANew(configurations) : ToANew<TResult>();
        }

        #endregion
    }
}