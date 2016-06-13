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
        {
            using (var planContext = new MappingContext(_mapperContext.RuleSets.CreateNew, _mapperContext))
            {
                return new MappingPlan<TSource, TResult>(planContext);
            }
        }
    }
}