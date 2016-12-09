namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal class OverwriteEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new OverwriteEnumerablePopulationStrategy();

        protected override Expression GetEnumerablePopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData)
        {
            if (builder.ElementTypesAreSimple)
            {
                if (mappingData.MapperData.TargetType.IsArray)
                {
                    builder.PopulateTargetVariableFromSourceObjectOnly();
                    return builder;
                }

                builder.AssignSourceVariableFrom(s => s.SourceItemsProjectedToTargetType());
                builder.AssignTargetVariable();
                builder.RemoveAllTargetItems();
                builder.AddNewItemsToTargetVariable(mappingData);

                return builder;
            }

            if (builder.ElementsAreIdentifiable)
            {
                builder.CreateCollectionData();
                builder.MapIntersection(mappingData);
                builder.AssignSourceVariableFrom(s => s.CollectionDataNewSourceItems());
                builder.AssignTargetVariable();
                builder.RemoveTargetItemsById();
                builder.AddNewItemsToTargetVariable(mappingData);

                return builder;
            }

            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.RemoveAllTargetItems();
            builder.AddNewItemsToTargetVariable(mappingData);

            return builder;
        }
    }
}
