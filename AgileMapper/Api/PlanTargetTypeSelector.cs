namespace AgileObjects.AgileMapper.Api
{
    using System.Linq;
    using Plans;

    internal class PlanTargetTypeSelector<TSource> : IPlanTargetTypeSelector<TSource>, IPlanTargetTypeAndRuleSetSelector<TSource>
    {
        private readonly MapperContext _mapperContext;

        internal PlanTargetTypeSelector(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public MappingPlanSet<TSource, TTarget> To<TTarget>()
        {
            return new MappingPlanSet<TSource, TTarget>(
                _mapperContext
                    .RuleSets
                    .All
                    .Select(GetMappingPlan<TTarget>)
                    .ToArray());
        }

        public MappingPlan<TSource, TResult> ToANew<TResult>()
            => GetMappingPlan<TResult>(_mapperContext.RuleSets.CreateNew);

        public MappingPlan<TSource, TTarget> OnTo<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Merge);

        public MappingPlan<TSource, TTarget> Over<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Overwrite);

        private MappingPlan<TSource, TTarget> GetMappingPlan<TTarget>(MappingRuleSet ruleSet)
        {
            var planContext = new MappingExecutor<TSource>(ruleSet, _mapperContext);

            return new MappingPlan<TSource, TTarget>(planContext);
        }
    }
}