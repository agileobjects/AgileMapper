namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Enables the selection of a derived result type to which to match a configured source type.
    /// </summary>
    /// <typeparam name="TSourceElement">
    /// The type of source object for which the derived type pair is being configured.
    /// </typeparam>
    /// <typeparam name="TResultElement">
    /// The type of result object for which the derived type pair is being configured.
    /// </typeparam>
    public interface IProjectionDerivedPairTargetTypeSpecifier<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Map the derived source type being configured to the derived result type specified by the type argument.
        /// </summary>
        /// <typeparam name="TDerivedResult">
        /// The derived result type to create for the configured derived source type.
        /// </typeparam>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of mappings from and to the source and 
        /// result type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> To<TDerivedResult>()
            where TDerivedResult : TResultElement;
    }
}