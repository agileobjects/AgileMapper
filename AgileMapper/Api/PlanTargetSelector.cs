namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Configuration.Inline;
    using Configuration;
    using Extensions;
    using Extensions.Internal;
    using ObjectPopulation;
    using Plans;
    using Queryables.Api;
    using static Plans.MappingPlanSettings.Default;
    
    internal class PlanTargetSelector<TSource> :
        IPlanTargetSelector<TSource>,
        IPlanTargetAndRuleSetSelector<TSource>,
        IProjectionPlanTargetSelector<TSource>
    {
        private readonly MapperContext _mapperContext;
        private readonly IQueryable<TSource> _exampleQueryable;

        public PlanTargetSelector(MapperContext mapperContext, IQueryable<TSource> exampleQueryable)
            : this(mapperContext)
        {
            _exampleQueryable = exampleQueryable;
        }

        internal PlanTargetSelector(MapperContext mapperContext)
        {
            _mapperContext = mapperContext.ThrowIfDisposed();
        }

        public MappingPlanSet To<TTarget>() 
            => To(configurations: Enumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>>.EmptyArray);

        public MappingPlanSet To<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return new(_mapperContext
                .RuleSets
                .All
                .Filter(_mapperContext, (mc, ruleSet) => ruleSet != mc.RuleSets.Project)
                .Project(configurations, (cs, rs) => GetMappingPlan(rs, EagerPlanned, cs))
                .ToArray());
        }

        public MappingPlan ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return GetMappingPlan(
                _mapperContext.RuleSets.CreateNew,
                EagerPlanned,
                configurations);
        }

        public MappingPlan OnTo<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return GetMappingPlan(
                _mapperContext.RuleSets.Merge,
                EagerPlanned,
                configurations);
        }

        public MappingPlan Over<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return GetMappingPlan(
                _mapperContext.RuleSets.Overwrite,
                EagerPlanned,
                configurations);
        }

        MappingPlan IProjectionPlanTargetSelector<TSource>.To<TResult>()
        {
            return GetMappingPlan(
                _mapperContext.QueryProjectionMappingContext,
                planContext => ObjectMappingDataFactory.ForProjection<TSource, TResult>(_exampleQueryable, planContext),
                Enumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>>.EmptyArray);
        }

        private MappingPlan GetMappingPlan<TTarget>(
            MappingRuleSet ruleSet,
            MappingPlanSettings settings,
            ICollection<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations)
        {
            return GetMappingPlan(
                new SimpleMappingContext(ruleSet, settings, _mapperContext),
                ObjectMappingDataFactory.ForRootFixedTypes<TSource, TTarget>,
                configurations);
        }

        private static MappingPlan GetMappingPlan<TTarget>(
            IMappingContext planContext,
            Func<IMappingContext, IObjectMappingData> mappingDataFactory,
            ICollection<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations)
        {
            if (configurations.Any())
            {
                InlineMappingConfigurator<TSource, TTarget>
#if NET35
                    .ConfigureMapperContext(configurations.Project(c => c.ToDlrExpression()), planContext);
#else
                    .ConfigureMapperContext(configurations, planContext);
#endif
            }

            var mappingData = mappingDataFactory.Invoke(planContext);

            return MappingPlan.For(mappingData);
        }
    }
}