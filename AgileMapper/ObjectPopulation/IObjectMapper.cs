namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using RepeatedMappings;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IObjectMapper : IObjectMapperFunc
    {
        Expression MappingExpression { get; }

        ObjectMapperData MapperData { get; }

        IEnumerable<IRepeatedMapperFunc> RepeatedMappingFuncs { get; }

        void CacheRepeatedMappingFuncs();

        bool IsStaticallyCacheable();

        void Reset();
    }
}