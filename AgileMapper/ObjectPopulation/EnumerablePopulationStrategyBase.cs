namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Extensions;

    internal abstract class EnumerablePopulationStrategyBase : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(IObjectMappingContext omc)
        {
            if (omc.SourceType.IsEnumerable())
            {
                return GetEnumerablePopulation(new EnumerablePopulationBuilder(omc));
            }

            return Constants.EmptyExpression;
        }

        protected abstract Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder);
    }
}
