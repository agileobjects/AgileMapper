namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for specifying the type of Dictionary mapping to perform.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the Dictionary to which the configurations will apply.
    /// </typeparam>
    public interface ISourceDictionaryTargetTypeSelector<TValue> : ISourceDictionarySettings<TValue>
    {
        /// <summary>
        /// Configure how this mapper performs mappings from Dictionaries in all MappingRuleSets 
        /// (create new, overwrite, etc), to the target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDictionaryMappingConfigurator with which to complete the configuration.</returns>
        ISourceDictionaryMappingConfigurator<TValue, TTarget> To<TTarget>();

        /// <summary>
        /// Configure how this mapper performs object creation mappings from Dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDictionaryMappingConfigurator with which to complete the configuration.</returns>
        ISourceDictionaryMappingConfigurator<TValue, TTarget> ToANew<TTarget>();

        /// <summary>
        /// Configure how this mapper performs OnTo (merge) mappings from Dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDictionaryMappingConfigurator with which to complete the configuration.</returns>
        ISourceDictionaryMappingConfigurator<TValue, TTarget> OnTo<TTarget>();

        /// <summary>
        /// Configure how this mapper performs Over (overwrite) mappings from Dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDictionaryMappingConfigurator with which to complete the configuration.</returns>
        ISourceDictionaryMappingConfigurator<TValue, TTarget> Over<TTarget>();
    }
}