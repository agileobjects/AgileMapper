﻿namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Configuration.Inline;
    using Configuration;
    using ObjectPopulation;
    using Extensions.Internal;
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

        public MappingPlan ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.CreateNew, configurations);

        public MappingPlan OnTo<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.Merge, configurations);

        public MappingPlan Over<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.Overwrite, configurations);

        MappingPlan IProjectionPlanTargetSelector<TSource>.To<TResult>()
        {
            return GetMappingPlan<TResult>(
                _mapperContext.QueryProjectionMappingContext,
                planContext => ObjectMappingDataFactory.ForProjection<TSource, TResult>(_exampleQueryable, planContext));
        }

        private MappingPlan GetMappingPlan<TTarget>(
            MappingRuleSet ruleSet,
            ICollection<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations)
        {
            var planContext = new SimpleMappingContext(ruleSet, _mapperContext)
            {
                AddUnsuccessfulMemberPopulations = true,
                LazyLoadRecursionMappingFuncs = false
            };

            return GetMappingPlan(
                planContext,
                ObjectMappingDataFactory.ForRootFixedTypes<TSource, TTarget>,
                configurations);
        }

        private static MappingPlan GetMappingPlan<TTarget>(
            IMappingContext planContext,
            Func<IMappingContext, IObjectMappingData> mappingDataFactory,
            ICollection<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations = null)
        {
            if (configurations?.Any() == true)
            {
                InlineMappingConfigurator<TSource, TTarget>
                    .ConfigureMapperContext(configurations, planContext);
            }

            var mappingData = mappingDataFactory.Invoke(planContext);

            return MappingPlan.For(mappingData);
        }
    }
}