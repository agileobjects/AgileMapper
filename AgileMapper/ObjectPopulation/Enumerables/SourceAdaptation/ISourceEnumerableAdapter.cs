namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.SourceAdaptation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Looping;

    internal interface ISourceEnumerableAdapter
    {
        Expression GetElementKey();

        Expression GetSourceValues();

        Expression GetSourceCountAccess();

        bool UseReadOnlyTargetWrapper { get; }

        Expression GetMappingShortCircuitOrNull();

        IPopulationLoopData GetPopulationLoopData();
    }
}