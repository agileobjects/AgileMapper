namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IObjectMapperFunc
    {
        LambdaExpression MappingLambda { get; }

        object Map(IObjectMappingData mappingData);
    }
}