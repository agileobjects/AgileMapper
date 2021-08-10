namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IRepeatedMapperFunc : IObjectMapperFuncBase
    {
        Type SourceType { get; }

        Type TargetType { get; }

        bool HasDerivedTypes { get; }

        LambdaExpression Mapping { get; }
    }
}