namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Enables chaining of configurations for the same source and target dictionary type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    public interface ITargetDictionaryMappingConfigContinuation<TSource, TValue>
    {
        /// <summary>
        /// Perform another configuration of how this mapper maps to and from the source and target dictionary 
        /// types being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        ITargetDictionaryMappingConfigurator<TSource, TValue> And { get; }
    }
}