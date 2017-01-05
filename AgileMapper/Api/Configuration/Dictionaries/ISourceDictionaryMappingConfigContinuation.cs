namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Enables chaining of configurations for the same dictionary and target type.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ISourceDictionaryMappingConfigContinuation<TValue, TTarget>
    {
        /// <summary>
        /// Perform another configuration of how this mapper maps to and from the dictionary and target types
        /// being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        ISourceDictionaryMappingConfigurator<TValue, TTarget> And { get; }
    }
}