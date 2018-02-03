namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring query projections from and to given source and target element Types, inline.
    /// </summary>
    /// <typeparam name="TSourceElement">The source element Type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result element Type to which the configuration should apply.</typeparam>
    public interface IFullProjectionInlineConfigurator<TSourceElement, TResultElement>
    {
    }
}
