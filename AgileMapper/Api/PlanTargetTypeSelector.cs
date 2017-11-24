namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Configuration.Inline;
    using Configuration;
    using ObjectPopulation;
    using Plans;

    internal class MappingPlanPlaceholderQueryable<T> : IQueryable<T>
    {
        public static readonly IQueryable<T> Default = new MappingPlanPlaceholderQueryable<T>();

        public Type ElementType => typeof(T);

        public Expression Expression => null;

        public IQueryProvider Provider => null;

        #region IEnumerable Members

        public IEnumerator<T> GetEnumerator() => Enumerable<T>.Empty.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    internal class PlanTargetTypeSelector<TSource> :
        IPlanTargetTypeSelector<TSource>,
        IPlanTargetTypeAndRuleSetSelector<TSource>
    {
        private readonly MapperContext _mapperContext;

        internal PlanTargetTypeSelector(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public MappingPlanSet To<TTarget>() => To<TTarget>(configurations: null);

        public MappingPlanSet To<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            // TODO: Include projection mapping plans:
            return new MappingPlanSet(
                _mapperContext
                    .RuleSets
                    .All
                    .Except(new[] { _mapperContext.RuleSets.Project })
                    .Select(rs => GetMappingPlan(rs, configurations))
                    .ToArray());
        }

        public MappingPlan ToANew<TResult>()
            => GetMappingPlan<TResult>(_mapperContext.RuleSets.CreateNew);

        public MappingPlan ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.CreateNew, configurations);

        public MappingPlan OnTo<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Merge);

        public MappingPlan OnTo<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.Merge, configurations);

        public MappingPlan Over<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Overwrite);

        public MappingPlan Over<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.Overwrite, configurations);

        public MappingPlan ProjectedTo<TResult>()
        {
            IObjectMappingData CreateProjectionMappingData(IMappingContext planContext)
            {
                return ObjectMappingDataFactory.ForProjection<TSource, TResult>(
                    MappingPlanPlaceholderQueryable<TSource>.Default,
                    planContext);
            }

            return GetMappingPlan<TResult>(
                _mapperContext.QueryProjectionMappingContext,
                CreateProjectionMappingData);
        }

        private MappingPlan GetMappingPlan<TTarget>(
            MappingRuleSet ruleSet,
            IEnumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations = null)
        {
            var planContext = new SimpleMappingContext(ruleSet, _mapperContext)
            {
                AddUnsuccessfulMemberPopulations = true
            };

            return GetMappingPlan(
                planContext,
                ObjectMappingDataFactory.ForRootFixedTypes<TSource, TTarget>,
                configurations);
        }

        private static MappingPlan GetMappingPlan<TTarget>(
            IMappingContext planContext,
            Func<IMappingContext, IObjectMappingData> mappingDataFactory,
            IEnumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations = null)
        {
            if (configurations != null)
            {
                InlineMappingConfigurator<TSource, TTarget>
                    .ConfigureMapperContext(configurations, planContext);
            }

            var mappingData = mappingDataFactory.Invoke(planContext);

            return MappingPlan.For(mappingData);
        }
    }
}