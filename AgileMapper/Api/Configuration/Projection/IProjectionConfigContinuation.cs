namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Enables chaining of configurations for the same source and result type.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result type to which the configuration should apply.</typeparam>
    public interface IProjectionConfigContinuation<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Perform another configuration of how this mapper projects to and from the source and result types
        /// being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        IFullProjectionConfigurator<TSourceElement, TResultElement> And { get; }

        /// <summary>
        /// Perform an alternative configuration of how this mapper projects to and from the source and result 
        /// types being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        IFullProjectionConfigurator<TSourceElement, TResultElement> But { get; }
    }
}