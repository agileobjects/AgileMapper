namespace AgileObjects.AgileMapper.Members.Population
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IMemberPopulationContext
    {
        IMemberMapperData MapperData { get; }

        Expression PopulateCondition { get; }
    }
}