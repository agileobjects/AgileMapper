namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;

    internal interface IObjectMapperFunc
    {
        Type SourceType { get; }

        Type TargetType { get; }

        LambdaExpression MappingLambda { get; }
    }
}