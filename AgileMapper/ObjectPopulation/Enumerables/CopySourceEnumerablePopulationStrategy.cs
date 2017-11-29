namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal class CopySourceEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        protected override Expression GetEnumerablePopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData)
        {
            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.AddNewItemsToTargetVariable(mappingData);

            return builder;
        }
    }
}