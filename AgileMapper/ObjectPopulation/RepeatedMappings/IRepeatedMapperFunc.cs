namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
    using System;

    internal interface IRepeatedMapperFunc : IObjectMapperFunc
    {
        Type SourceType { get; }

        Type TargetType { get; }

    }
}