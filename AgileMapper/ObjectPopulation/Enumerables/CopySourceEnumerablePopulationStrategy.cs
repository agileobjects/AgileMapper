namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal struct CopySourceEnumerablePopulationStrategy : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(EnumerablePopulationBuilder builder, IObjectMappingData enumerableMappingData)
        {
            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.AddNewItemsToTargetVariable(enumerableMappingData);

            return builder;
        }
    }
}