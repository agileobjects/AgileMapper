namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class CopySourceEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new CopySourceEnumerablePopulationStrategy();

        protected override Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder)
        {
            builder.AddNullTargetShortCircuitIfAppropriate();
            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.AddNewItemsToTargetVariable();

            return builder;
        }
    }
}