namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring a mapping based on the preceding condition.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result type to which the configuration should apply.</typeparam>
    public interface IConditionalRootProjectionConfigurator<TSourceElement, TResultElement> :
        IRootProjectionConfigurator<TSourceElement, TResultElement>
    {
    }
}