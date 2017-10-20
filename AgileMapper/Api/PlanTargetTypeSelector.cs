namespace AgileObjects.AgileMapper.Api
{
    using System.Linq;
    using Plans;

    internal class PlanTargetTypeSelector<TSource> : IPlanTargetTypeSelector, IPlanTargetTypeAndRuleSetSelector
    {
        private readonly MapperContext _mapperContext;

        internal PlanTargetTypeSelector(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public string To<TTarget>()
        {
            return new MappingPlanSet<TSource, TTarget>(
                _mapperContext
                    .RuleSets
                    .All
                    .Select(GetMappingPlan<TTarget>)
                    .ToArray());
        }

        public string ToANew<TResult>()
            => GetMappingPlan<TResult>(_mapperContext.RuleSets.CreateNew);

        public string OnTo<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Merge);

        public string Over<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Overwrite);

        private MappingPlan<TSource, TTarget> GetMappingPlan<TTarget>(MappingRuleSet ruleSet)
        {
            var planContext = new MappingExecutor<TSource>(ruleSet, _mapperContext);

            return new MappingPlan<TSource, TTarget>(planContext);
        }
    }
}