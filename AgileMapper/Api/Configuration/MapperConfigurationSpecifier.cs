namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    /// <summary>
    /// Provides options for specifying <see cref="MapperConfiguration"/> instances with which to
    /// perform Mapper configuration.
    /// </summary>
    public class MapperConfigurationSpecifier
    {
        private readonly MappingConfigStartingPoint _configStartingPoint;

        internal MapperConfigurationSpecifier(MappingConfigStartingPoint configStartingPoint)
        {
            _configStartingPoint = configStartingPoint;
        }

        /// <summary>
        /// Apply the configuration in the <see cref="MapperConfiguration"/> of the given
        /// <typeparamref name="TConfiguration"/> instance.
        /// </summary>
        /// <typeparam name="TConfiguration"></typeparam>
        public void From<TConfiguration>()
            where TConfiguration : MapperConfiguration, new()
        {
            var configuration = new TConfiguration();

            configuration.ApplyTo(_configStartingPoint);
        }
    }
}