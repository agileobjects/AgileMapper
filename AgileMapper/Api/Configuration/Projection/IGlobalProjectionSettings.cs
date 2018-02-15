namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for globally configuring how all mappers will perform query projections.
    /// </summary>
    public interface IGlobalProjectionSettings
    {
        /// <summary>
        /// Gets a link back to the full <see cref="IProjectionConfigStartingPoint"/>, for api fluency.
        /// </summary>
        IProjectionConfigStartingPoint AndWhenMapping { get; }
    }
}