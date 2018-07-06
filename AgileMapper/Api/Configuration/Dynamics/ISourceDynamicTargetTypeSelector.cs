#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    /// <summary>
    /// Provides options for specifying the type of ExpandoObject mapping to perform.
    /// </summary>
    public interface ISourceDynamicTargetTypeSelector : ISourceDynamicSettings
    {
        /// <summary>
        /// Configure how this mapper performs mappings from ExpandoObjects in all MappingRuleSets 
        /// (create new, overwrite, etc), to the target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDynamicMappingConfigurator with which to complete the configuration.</returns>
        ISourceDynamicMappingConfigurator<TTarget> To<TTarget>();

        /// <summary>
        /// Configure how this mapper performs object creation mappings from ExpandoObjects to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDynamicMappingConfigurator with which to complete the configuration.</returns>
        ISourceDynamicMappingConfigurator<TTarget> ToANew<TTarget>();

        /// <summary>
        /// Configure how this mapper performs OnTo (merge) mappings from ExpandoObjects to the target 
        /// type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDynamicMappingConfigurator with which to complete the configuration.</returns>
        ISourceDynamicMappingConfigurator<TTarget> OnTo<TTarget>();

        /// <summary>
        /// Configure how this mapper performs Over (overwrite) mappings from ExpandoObjects to the target 
        /// type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDynamicMappingConfigurator with which to complete the configuration.</returns>
        ISourceDynamicMappingConfigurator<TTarget> Over<TTarget>();
    }
}
#endif