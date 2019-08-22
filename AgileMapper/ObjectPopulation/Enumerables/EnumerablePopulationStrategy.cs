namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal delegate Expression EnumerablePopulationStrategy(
        EnumerablePopulationBuilder builder,
        IObjectMappingData enumerableMappingData);
}
