namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using Recursion;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IObjectMapper : IObjectMapperFunc
    {
        Expression MappingExpression { get; }

        ObjectMapperData MapperData { get; }

        IEnumerable<IRecursionMapperFunc> RecursionMapperFuncs { get; }

        void CacheRecursionMapperFuncs();

        bool IsStaticallyCacheable();

        void Reset();
    }
}