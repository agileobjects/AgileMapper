namespace AgileObjects.AgileMapper.Configuration
{
    using Api.Configuration;

    /// <summary>
    /// Base class for multiple, dedicated mapper configuration classes.
    /// </summary>
    public abstract class MapperConfiguration
    {
        internal void ApplyTo(MappingConfigStartingPoint configStartingPoint) => Configure(configStartingPoint);

        /// <summary>
        /// Configure how mappings should be performed.
        /// </summary>
        /// <param name="whenMapping">The <see cref="MappingConfigStartingPoint"/> with which to configure mappings.</param>
        protected abstract void Configure(MappingConfigStartingPoint whenMapping);
    }
}
