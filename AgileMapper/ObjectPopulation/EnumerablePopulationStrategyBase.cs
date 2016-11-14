namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal abstract class EnumerablePopulationStrategyBase : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(IObjectMappingData enumerableMappingData)
        {
            return GetEnumerablePopulation(
                enumerableMappingData.MapperData.EnumerablePopulationBuilder,
                enumerableMappingData);
        }

        protected abstract Expression GetEnumerablePopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData);
    }
}
