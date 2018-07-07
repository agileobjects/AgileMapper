namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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