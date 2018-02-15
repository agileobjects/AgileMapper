namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring a mapping based on the preceding condition.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result type to which the configuration should apply.</typeparam>
    public interface IConditionalRootProjectionConfigurator<TSourceElement, TResultElement> :
        IRootProjectionConfigurator<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Project the source Type being configured to the derived result type specified by 
        /// <typeparamref name="TDerivedResult"/> if the preceding condition evaluates to true.
        /// </summary>
        /// <typeparam name="TDerivedResult">The derived result type to create.</typeparam>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of mappings from and to the source 
        /// and result type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> MapTo<TDerivedResult>()
            where TDerivedResult : TResultElement;

        /// <summary>
        /// Project the result Type being configured to null if the preceding condition evaluates to true.
        /// </summary>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of mappings from and to the 
        /// source and result Type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> MapToNull();
    }
}