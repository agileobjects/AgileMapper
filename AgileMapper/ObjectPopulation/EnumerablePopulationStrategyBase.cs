namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal abstract class EnumerablePopulationStrategyBase : IEnumerablePopulationStrategy
    {
        public virtual bool DiscardExistingValues => false;

        public Expression GetPopulation(ObjectMapperData mapperData)
        {
            if (mapperData.SourceMember.IsEnumerable)
            {
                return GetEnumerablePopulation(mapperData.EnumerablePopulationBuilder);
            }

            return Constants.EmptyExpression;
        }

        protected abstract Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder);
    }
}
