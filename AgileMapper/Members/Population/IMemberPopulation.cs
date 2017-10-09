namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;

    internal interface IMemberPopulation
    {
        IMemberMapperData MapperData { get; }

        bool IsSuccessful { get; }

        Expression GetBinding();

        Expression GetPopulation();
    }
}