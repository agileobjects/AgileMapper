namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Enables chaining of configurations for the same source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IMappingConfigContinuation<TSource, TTarget>
    {
        /// <summary>
        /// Perform another configuration of how this mapper maps to and from the source and target types
        /// being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        IFullMappingConfigurator<TSource, TTarget> And { get; }

        /// <summary>
        /// Perform an alternative configuration of how this mapper maps to and from the source and target types
        /// being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        IFullMappingConfigurator<TSource, TTarget> But { get; }
    }
}