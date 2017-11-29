namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal class MergeEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        protected override Expression GetEnumerablePopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData)
        {
            if (builder.ElementTypesAreSimple)
            {
                builder.AssignSourceVariableFrom(s => s.SourceItemsProjectedToTargetType().ExcludingTargetItems());
                builder.AssignTargetVariable();
                builder.AddNewItemsToTargetVariable(mappingData);

                return builder;
            }

            if (builder.ElementsAreIdentifiable)
            {
                builder.CreateCollectionData();
                builder.MapIntersection(mappingData);
                builder.AssignSourceVariableFrom(s => s.CollectionDataNewSourceItems());
                builder.AssignTargetVariable();
                builder.AddNewItemsToTargetVariable(mappingData);

                return builder;
            }

            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.AddNewItemsToTargetVariable(mappingData);

            return builder;
        }
    }
}