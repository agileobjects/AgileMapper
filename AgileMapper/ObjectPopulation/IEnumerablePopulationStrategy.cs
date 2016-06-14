namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IEnumerablePopulationStrategy
    {
        Expression GetPopulation(IObjectMappingContext omc);
    }
}
