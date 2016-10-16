namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal interface IEnumerablePopulationStrategy
    {
        bool DiscardExistingValues { get; }

        Expression GetPopulation(ObjectMapperData mapperData);
    }
}
