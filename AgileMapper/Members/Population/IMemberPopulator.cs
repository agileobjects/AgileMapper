namespace AgileObjects.AgileMapper.Members.Population
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IMemberPopulator
    {
        IMemberMapperData MapperData { get; }

        bool CanPopulate { get; }

        Expression GetPopulation();
    }
}