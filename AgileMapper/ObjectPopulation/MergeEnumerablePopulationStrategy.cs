namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class MergeEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new MergeEnumerablePopulationStrategy();

        protected override Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder)
        {
            if (builder.ElementTypesAreSimple)
            {
                builder.AssignSourceVariableFrom(s => s.SourceItemsProjectedToTargetType().ExcludingTargetItems());
                builder.AssignTargetVariable();
                builder.AddNewItemsToTargetVariable();

                return builder;
            }

            if (builder.ElementTypesAreIdentifiable)
            {
                builder.CreateCollectionData();
                builder.MapIntersection();
                builder.AssignSourceVariableFrom(s => s.CollectionDataNewSourceItems());
                builder.AssignTargetVariable();
                builder.AddNewItemsToTargetVariable();

                return builder;
            }

            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.AddNewItemsToTargetVariable();

            return builder;
        }
    }
}