namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;
    using DataSources;

    internal interface IMemberPopulationContext
    {
        IMemberMapperData MapperData { get; }

        bool IsSuccessful { get; }

        DataSourceSet DataSources { get; }

        Expression PopulateCondition { get; }
    }
}