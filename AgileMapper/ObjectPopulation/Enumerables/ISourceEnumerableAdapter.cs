namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal interface ISourceEnumerableAdapter
    {
        Expression GetSourceValues();

        Expression GetSourceCountAccess();

        bool UseReadOnlyTargetWrapper { get; }

        IPopulationLoopData GetPopulationLoopData();
    }
}