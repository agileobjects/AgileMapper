#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    /// <summary>
    /// Enables chaining of configurations for an ExpandoObject to the same target type.
    /// </summary>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ISourceDynamicMappingConfigContinuation<TTarget>
    {
        /// <summary>
        /// Perform another configuration of how this mapper maps from an ExpandoObject to the target type
        /// being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        ISourceDynamicMappingConfigurator<TTarget> And { get; }
    }
}
#endif