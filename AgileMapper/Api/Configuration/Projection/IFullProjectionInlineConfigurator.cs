namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring query projections from and to given source and result element Types, inline.
    /// </summary>
    /// <typeparam name="TSourceElement">The source element Type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result element Type to which the configuration should apply.</typeparam>
    public interface IFullProjectionInlineConfigurator<TSourceElement, TResultElement> :
        IFullProjectionConfigurator<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Configure how this mapper performs a projection, inline. Use this property to switch from 
        /// configuration of the root Types on which the projection is being performed to configuration 
        /// of any other Types.
        /// </summary>
        MappingConfigStartingPoint WhenMapping { get; }

        /// <summary>
        /// Throw an exception upon execution of this statement if the projection being configured has any result 
        /// members which will not be mapped, projects from a source enum to a target enum which does not support 
        /// all of its values, or includes complex types which cannot be constructed. Use calls to this method to 
        /// validate a mapping plan; remove them in production 
        /// code.
        /// </summary>
        void ThrowNowIfMappingPlanIsIncomplete();
    }
}
