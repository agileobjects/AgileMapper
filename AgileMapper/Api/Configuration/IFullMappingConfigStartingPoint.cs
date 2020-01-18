namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Provides options for starting a fluent configuration setting for mappings from and to a given
    /// source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IFullMappingConfigStartingPoint<TSource, TTarget>
    {
        /// <summary>
        /// Configure this mapper to perform an action before a different specified action.
        /// </summary>
        PreEventMappingConfigStartingPoint<TSource, TTarget> Before { get; }

        /// <summary>
        /// Configure this mapper to perform an action after a different specified action.
        /// </summary>
        PostEventMappingConfigStartingPoint<TSource, TTarget> After { get; }

        /// <summary>
        /// Configure a derived target type to which to map instances of the given derived source type.
        /// </summary>
        /// <typeparam name="TDerivedSource">
        /// The derived source type for which to configure a matching derived target type.
        /// </typeparam>
        /// <returns>
        /// A IMappingDerivedPairTargetTypeSpecifier with which to specify the matching derived target type.
        /// </returns>
        IMappingDerivedPairTargetTypeSpecifier<TSource, TTarget> Map<TDerivedSource>()
            where TDerivedSource : TSource;

        /// <summary>
        /// Map the source type being configured to the derived target type specified by 
        /// <typeparamref name="TDerivedTarget"/>.
        /// </summary>
        /// <typeparam name="TDerivedTarget">The derived target type to create.</typeparam>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> MapTo<TDerivedTarget>()
            where TDerivedTarget : TTarget;
    }
}