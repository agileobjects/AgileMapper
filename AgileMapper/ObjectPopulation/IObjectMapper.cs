namespace AgileObjects.AgileMapper.ObjectPopulation;

using System.Collections.Generic;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using RepeatedMappings;

internal interface IObjectMapper : IObjectMapperFunc
{
    ObjectMapperData MapperData { get; }

    LambdaExpression GetMappingLambda();

    IEnumerable<IRepeatedMapperFunc> RepeatedMappingFuncs { get; }

    void CacheRepeatedMappingFuncs();

    bool IsStaticallyCacheable();

    object MapSubObject(MappingExecutionContextBase2 context);

    object MapRepeated(MappingExecutionContextBase2 context);

    void Reset();
}