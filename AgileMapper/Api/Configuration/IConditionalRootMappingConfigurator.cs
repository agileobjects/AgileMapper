namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Provides options for configuring a mapping based on the preceding condition.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IConditionalRootMappingConfigurator<TSource, TTarget>
        : IRootMappingConfigurator<TSource, TTarget>
    {
        /// <summary>
        /// Map the source type being configured to the derived target type specified by 
        /// <typeparamref name="TDerivedTarget"/> if the preceding condition evaluates to true.
        /// </summary>
        /// <typeparam name="TDerivedTarget">The derived target type to create.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> MapTo<TDerivedTarget>()
            where TDerivedTarget : TTarget;

        /// <summary>
        /// Map the target type being configured to null if the preceding condition evaluates to true.
        /// </summary>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> MapToNull();
    }
}