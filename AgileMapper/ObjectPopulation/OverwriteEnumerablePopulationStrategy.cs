namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class OverwriteEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new OverwriteEnumerablePopulationStrategy();

        protected override Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder)
        {
            if (builder.TypesAreIdentifiable)
            {
                var removeExistingObjects = builder
                    .ExcludeSourceById()
                    .CallToArray()
                    .RemoveResultsFromTarget();

                var updateExistingObjects = builder
                    .IntersectTargetById()
                    .MapResultsToTarget();

                var addNewObjects = builder
                    .ExcludeTargetById()
                    .ProjectToTargetType()
                    .CallToArray()
                    .AddResultsToTarget();

                return Expression.Block(
                    removeExistingObjects,
                    updateExistingObjects,
                    addNewObjects,
                    Constants.EmptyExpression);
            }

            var removeExistingItems = builder.ClearTarget();

            var addSourceItemsToTarget = builder.ProjectToTargetType().AddResultsToTarget();

            return Expression.Block(removeExistingItems, addSourceItemsToTarget, Constants.EmptyExpression);
        }
    }
}
