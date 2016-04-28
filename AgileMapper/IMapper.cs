namespace AgileObjects.AgileMapper
{
    using System;
    using Api;
    using Api.Configuration;

    public interface IMapper : IDisposable
    {
        ConfigStartingPoint When { get; }

        ResultTypeSelector<TSource> Map<TSource>(TSource source);
    }
}