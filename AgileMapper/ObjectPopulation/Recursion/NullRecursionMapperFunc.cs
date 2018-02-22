namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System;
    using System.Linq.Expressions;

    internal class NullRecursionMapperFunc : IRecursionMapperFunc
    {
        public static readonly IRecursionMapperFunc Instance = new NullRecursionMapperFunc();

        public Type SourceType => null;

        public Type TargetType => null;

        public LambdaExpression MappingLambda => null;

        public object Map(IObjectMappingData mappingData) => null;
    }
}