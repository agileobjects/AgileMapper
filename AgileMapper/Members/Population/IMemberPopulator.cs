namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;

    internal interface IMemberPopulator
    {
        IMemberMapperData MapperData { get; }

        bool CanPopulate { get; }

        Expression GetPopulation();
    }
}