namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class ProjectSourceEnumerablePopulationStrategy
    {
        public static Expression Create(
            EnumerablePopulationBuilder builder,
            IObjectMappingData enumerableMappingData)
        {
            builder.PopulateTargetVariableFromSourceObjectOnly(enumerableMappingData);

            return builder;
        }
    }
}