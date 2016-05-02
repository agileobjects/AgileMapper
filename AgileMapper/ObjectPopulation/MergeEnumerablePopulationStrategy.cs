namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class MergeEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new MergeEnumerablePopulationStrategy();

        protected override Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder)
        {
            if (builder.TypesAreIdentifiable)
            {
                var updateExistingObjects = builder
                    .IntersectTargetById()
                    .MapResultsToTarget();

                var addNewObjects = builder
                    .ExcludeTargetById()
                    .ProjectToTargetType()
                    .CallToArray()
                    .AddResultsToTarget();

                return Expression.Block(
                    updateExistingObjects,
                    addNewObjects,
                    Constants.EmptyExpression);
            }

            return builder
                .ProjectToTargetType()
                .ExcludeTarget()
                .CallToArray()
                .AddResultsToTarget();
        }
    }
}