namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;

    internal interface IMemberPopulationFactory
    {
        Expression GetPopulation(IMemberPopulationContext context);
    }
}