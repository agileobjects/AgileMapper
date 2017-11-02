namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;

    internal interface IRecursionMapperFunc : IObjectMapperFunc
    {
        Type SourceType { get; }

        Type TargetType { get; }

    }
}