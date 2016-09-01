namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Provides options to configure a mapping based on the preceding condition.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IConditionalRootMappingConfigurator<TSource, TTarget>
        : IRootMappingConfigurator<TSource, TTarget>
    {
        /// <summary>
        /// Map the source type being configured to the derived target type specified by the type argument if
        /// the preceding condition evaluates to true.
        /// </summary>
        /// <typeparam name="TDerivedTarget">The derived target type to create.</typeparam>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        MappingConfigContinuation<TSource, TTarget> MapTo<TDerivedTarget>()
            where TDerivedTarget : TTarget;
    }
}