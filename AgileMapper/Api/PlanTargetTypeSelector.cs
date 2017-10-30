﻿namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Configuration.Inline;
    using Configuration;
    using Plans;

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
            return new MappingPlanSet(
                _mapperContext
                    .RuleSets
                    .All
                    .Select(rs => GetMappingPlan(rs, configurations))
                    .ToArray());
        }

        public MappingPlan<TSource, TResult> ToANew<TResult>()
            => GetMappingPlan<TResult>(_mapperContext.RuleSets.CreateNew);

        public MappingPlan<TSource, TResult> ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.CreateNew, configurations);

        public MappingPlan<TSource, TTarget> OnTo<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Merge);

        public MappingPlan<TSource, TTarget> OnTo<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.Merge, configurations);

        public MappingPlan<TSource, TTarget> Over<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Overwrite);

        public MappingPlan<TSource, TTarget> Over<TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
            => GetMappingPlan(_mapperContext.RuleSets.Overwrite, configurations);

        private MappingPlan<TSource, TTarget> GetMappingPlan<TTarget>(
            MappingRuleSet ruleSet,
            IEnumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations = null)
        {
            var planContext = new SimpleMappingContext(ruleSet, _mapperContext);

            if (configurations != null)
            {
                InlineMappingConfigurator<TSource, TTarget>
                    .ConfigureMapperContext(configurations, planContext);
            }

            return new MappingPlan<TSource, TTarget>(planContext);
        }
    }
}