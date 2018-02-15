namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Enables the selection of a derived target type to which to match a configured source type.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of source object for which the derived type pair is being configured.
    /// </typeparam>
    /// <typeparam name="TTarget">
    /// The type of target object for which the derived type pair is being configured.
    /// </typeparam>
    public interface IMappingDerivedPairTargetTypeSpecifier<TSource, TTarget>
    {
        /// <summary>
        /// Map the derived source type being configured to the derived target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TDerivedTarget">
        /// The derived target type to create for the configured derived source type.
        /// </typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> To<TDerivedTarget>()
            where TDerivedTarget : TTarget;
    }
}