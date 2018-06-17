namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal struct OverwriteEnumerablePopulationStrategy : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData enumerableMappingData)
        {
            if (builder.ElementTypesAreSimple)
            {
                if (builder.TargetTypeHelper.IsReadOnly)
                {
                    builder.PopulateTargetVariableFromSourceObjectOnly();
                    return builder;
                }

                builder.AssignSourceVariableFrom(s => s.SourceItemsProjectedToTargetType());
                builder.AssignTargetVariable();
                builder.RemoveAllTargetItems();
                builder.AddNewItemsToTargetVariable(enumerableMappingData);

                return builder;
            }

            if (builder.ElementsAreIdentifiable)
            {
                builder.CreateCollectionData();
                builder.MapIntersection(enumerableMappingData);
                builder.AssignSourceVariableFrom(s => s.CollectionDataNewSourceItems());
                builder.AssignTargetVariable();
                builder.RemoveTargetItemsById();
                builder.AddNewItemsToTargetVariable(enumerableMappingData);

                return builder;
            }

            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.RemoveAllTargetItems();
            builder.AddNewItemsToTargetVariable(enumerableMappingData);

            return builder;
        }
    }
}
