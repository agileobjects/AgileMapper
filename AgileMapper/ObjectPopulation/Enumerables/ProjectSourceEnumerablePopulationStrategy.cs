namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal class ProjectSourceEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new ProjectSourceEnumerablePopulationStrategy();

        protected override Expression GetEnumerablePopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData)
        {
            builder.PopulateTargetVariableFromSourceObjectOnly(mappingData);

            return builder;
        }
    }
}