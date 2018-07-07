namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface ISourceEnumerableAdapter
    {
        Expression GetSourceValues();

        Expression GetSourceCountAccess();

        bool UseReadOnlyTargetWrapper { get; }

        Expression GetMappingShortCircuitOrNull();

        IPopulationLoopData GetPopulationLoopData();
    }
}