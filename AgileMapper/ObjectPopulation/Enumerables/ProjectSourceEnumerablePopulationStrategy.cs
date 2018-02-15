namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal class ProjectSourceEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        protected override Expression GetEnumerablePopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData)
        {
            builder.PopulateTargetVariableFromSourceObjectOnly(mappingData);

            return builder;
        }
    }
}