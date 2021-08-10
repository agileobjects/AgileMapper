namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IObjectMapperFuncBase
    {
        object Map(IObjectMappingData mappingData);
    }

    internal interface IObjectMapperFunc : IObjectMapperFuncBase
    {
        Expression Mapping { get; }
    }
}