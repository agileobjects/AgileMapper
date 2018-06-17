namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal struct ProjectSourceEnumerablePopulationStrategy : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData enumerableMappingData)
        {
            builder.PopulateTargetVariableFromSourceObjectOnly(enumerableMappingData);

            return builder;
        }
    }
}