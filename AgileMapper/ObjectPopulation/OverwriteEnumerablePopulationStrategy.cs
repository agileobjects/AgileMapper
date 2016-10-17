namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class OverwriteEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new OverwriteEnumerablePopulationStrategy();

        public override bool DiscardExistingValues => true;

        protected override Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder)
        {
            if (builder.ElementTypesAreSimple)
            {
                if (builder.TargetIsReadOnly)
                {
                    builder.PopulateTargetVariableFromSourceObjectOnly();
                    return builder;
                }

                builder.AssignSourceVariableFrom(s => s.SourceItemsProjectedToTargetType());
                builder.AssignTargetVariable();
                builder.RemoveAllTargetItems();
                builder.AddNewItemsToTargetVariable();

                return builder;
            }

            if (builder.ElementTypesAreIdentifiable)
            {
                builder.CreateCollectionData();
                builder.MapIntersection();
                builder.AssignSourceVariableFrom(s => s.CollectionDataNewSourceItems());
                builder.AssignTargetVariable();
                builder.RemoveTargetItemsById();
                builder.AddNewItemsToTargetVariable();

                return builder;
            }

            builder.AssignSourceVariableFrom(s => s.SourceItemsProjectedToTargetType());
            builder.AssignTargetVariable();
            builder.RemoveAllTargetItems();
            builder.AddNewItemsToTargetVariable();

            return builder;
        }
    }
}
