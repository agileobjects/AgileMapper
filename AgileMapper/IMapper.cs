namespace AgileObjects.AgileMapper
{
    using System;
    using Api;
    using Api.Configuration;

    public interface IMapper : IDisposable
    {
        PostEventConfigStartingPoint After { get; }

        MappingConfigStartingPoint WhenMapping { get; }

        ResultTypeSelector<TSource> Map<TSource>(TSource source);
    }
}