namespace AgileObjects.AgileMapper.Members.Population
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;

    internal interface IMemberPopulationContext
    {
        IMemberMapperData MapperData { get; }

        bool IsSuccessful { get; }

        DataSourceSet DataSources { get; }

        Expression PopulateCondition { get; }
    }
}