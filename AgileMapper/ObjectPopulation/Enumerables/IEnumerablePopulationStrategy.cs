namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal interface IEnumerablePopulationStrategy
    {
        Expression GetPopulation(IObjectMappingData mappingData);
    }
}
