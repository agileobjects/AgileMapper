namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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