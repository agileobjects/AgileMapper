namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IEnumerablePopulationStrategy
    {
        Expression GetPopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData enumerableMappingData);
    }
}
