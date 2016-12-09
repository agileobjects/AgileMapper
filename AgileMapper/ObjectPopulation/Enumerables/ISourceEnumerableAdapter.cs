namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal interface ISourceEnumerableAdapter
    {
        Expression GetSourceValue();

        Expression GetSourceValues();

        Expression GetSourceCountAccess();

        IPopulationLoopData GetPopulationLoopData();
    }
}