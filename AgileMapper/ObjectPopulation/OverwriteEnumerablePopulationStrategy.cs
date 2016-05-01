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
                    Expression.Empty());
            }

            if (builder.ObjectMappingContext.TargetMember.Type.IsArray)
            {
                // No mapping needed, just overwrite the existing object:
                return Expression.Empty();
            }

            var removeExistingItems = builder.ClearTarget();
            var addSourceItemsToTarget = builder.AddResultsToTarget();

            return Expression.Block(removeExistingItems, addSourceItemsToTarget, Expression.Empty());

        }
    }
}
