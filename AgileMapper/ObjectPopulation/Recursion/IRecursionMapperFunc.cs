namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System;

    internal interface IRecursionMapperFunc : IObjectMapperFunc
    {
        Type SourceType { get; }

        Type TargetType { get; }

    }
}