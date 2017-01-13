namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System.Linq.Expressions;

    internal class CopySourceEnumerableDictionaryPopulationStrategy : DictionaryPopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new CopySourceEnumerableDictionaryPopulationStrategy();

        protected override Expression GetDictionaryPopulation(
            DictionaryPopulationBuilder builder,
            IObjectMappingData mappingData)
        {
            if (builder.HasSourceEnumerable)
            {
                builder.AssignSourceVariableFromSourceObject();
            }

            builder.AddItems(mappingData);

            return builder;
        }
    }
}