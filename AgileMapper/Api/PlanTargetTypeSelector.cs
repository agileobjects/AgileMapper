namespace AgileObjects.AgileMapper.Api
{
    using Plans;

    public class PlanTargetTypeSelector<TSource>
    {
        private readonly MapperContext _mapperContext;

        internal PlanTargetTypeSelector(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public string ToANew<TResult>() where TResult : class
            => GetMappingPlan<TResult>(_mapperContext.RuleSets.CreateNew);

        public string OnTo<TTarget>() where TTarget : class
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Merge);

        private string GetMappingPlan<TTarget>(MappingRuleSet ruleSet)
        {
            using (var planContext = new MappingContext(ruleSet, _mapperContext))
            {
                return new MappingPlan<TSource, TTarget>(planContext);
            }
        }
    }
}