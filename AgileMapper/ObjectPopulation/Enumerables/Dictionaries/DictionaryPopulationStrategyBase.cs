namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System.Linq.Expressions;

    internal abstract class DictionaryPopulationStrategyBase : EnumerablePopulationStrategyBase
    {
        protected override Expression GetEnumerablePopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData)
        {
            var dictionaryPopulationBuilder = new DictionaryPopulationBuilder(builder);

            return GetDictionaryPopulation(dictionaryPopulationBuilder, mappingData);
        }

        protected abstract Expression GetDictionaryPopulation(
            DictionaryPopulationBuilder builder,
            IObjectMappingData mappingData);
    }
}