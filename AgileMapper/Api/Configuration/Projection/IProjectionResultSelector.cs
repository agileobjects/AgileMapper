namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for specifying the target type and mapping rule set to which the configuration should
    /// apply.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type being configured.</typeparam>
    public interface IProjectionResultSelector<TSourceElement>
    {
        /// <summary>
        /// Configure how this mapper performs query projections from the source Type being configured to the 
        /// result Type specified by the given <typeparamref name="TResultElement"/> argument.
        /// </summary>
        /// <typeparam name="TResultElement">The result Type to which the configuration will apply.</typeparam>
        /// <returns>An IFullProjectionConfigurator with which to complete the configuration.</returns>
        IFullProjectionConfigurator<TSourceElement, TResultElement> ProjectedTo<TResultElement>();
    }
}