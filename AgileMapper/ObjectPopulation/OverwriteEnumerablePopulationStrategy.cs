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
                MergeEnumerablePopulationStrategy.MergeEnumerables(
                    builder,
                    b => b.MapIntersection(),
                    b => b.RemoveTargetItemsById());

                return builder;
            }

            builder.ProjectToTargetType().AssignValueToVariable();
            builder.IfTargetNotNull(b => b.ReplaceTargetItems());

            return builder;
        }
    }
}
