#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    /// <summary>
    /// Enables chaining of configurations for the same source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    public interface ITargetDynamicMappingConfigContinuation<TSource>
    {
        /// <summary>
        /// Perform another configuration of how this mapper maps to and from the source and target types 
        /// being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        ITargetDynamicMappingConfigurator<TSource> And { get; }
    }
}
#endif