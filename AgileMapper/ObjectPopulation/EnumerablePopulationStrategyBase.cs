namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal abstract class EnumerablePopulationStrategyBase : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(Expression targetVariableValue, IObjectMappingContext omc)
            => GetEnumerablePopulation(new EnumerablePopulationBuilder(omc));

        protected abstract Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder);
    }
}
