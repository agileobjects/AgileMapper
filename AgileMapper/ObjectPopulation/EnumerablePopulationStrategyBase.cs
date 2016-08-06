namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal abstract class EnumerablePopulationStrategyBase : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(IObjectMappingContext omc)
        {
            if (omc.SourceMember.IsEnumerable)
            {
                return GetEnumerablePopulation(omc.EnumerablePopulationBuilder);
            }

            return Constants.EmptyExpression;
        }

        protected abstract Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder);
    }
}
