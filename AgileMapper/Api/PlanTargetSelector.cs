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
                .RuleSets.All
                .Filter(_mapperContext, (mc, ruleSet) => ruleSet != mc.RuleSets.Project)
                .Project(configurations, (cfg, rs) => GetMappingPlan(rs, cfg))
                .ToArray());
        }

        public MappingPlan ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return GetMappingPlan(
                _mapperContext.RuleSets.CreateNew,
                configurations);
        }

        public MappingPlan OnTo<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return GetMappingPlan(
                _mapperContext.RuleSets.Merge,
                configurations);
        }

        public MappingPlan Over<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return GetMappingPlan(
                _mapperContext.RuleSets.Overwrite,
                configurations);
        }

        MappingPlan IProjectionPlanTargetSelector<TSource>.To<TResult>()
        {
            return GetMappingPlan<TResult>(
                new ProjectionPlanObjectMapperFactoryData<TSource, TResult>(
                    _exampleQueryable,
                    _mapperContext));
        }

        private MappingPlan GetMappingPlan<TTarget>(
            MappingRuleSet ruleSet,
            ICollection<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations)
        {
            var factoryData = new PlanObjectMapperFactoryData<TSource, TTarget>(ruleSet, _mapperContext);

            if (configurations.Any())
            {
                InlineMappingConfigurator<TSource, TTarget>
                    .ConfigureMapperContext(
#if NET35
                        configurations.Project(c => c.ToDlrExpression()),
#else
                        configurations,
#endif
                        factoryData);
            }

            return GetMappingPlan<TTarget>(factoryData);
        }

        private MappingPlan GetMappingPlan<TTarget>(IObjectMapperFactoryData objectMapperFactoryData)
        {
            var mapper = _mapperContext
                .ObjectMapperFactory
                .GetOrCreateRoot<TSource, TTarget>(objectMapperFactoryData);

            return new MappingPlan(mapper);
        }
    }
}