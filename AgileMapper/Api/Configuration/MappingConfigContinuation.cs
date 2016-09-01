namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    /// <summary>
    /// Enables chaining of configurations for the same source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public class MappingConfigContinuation<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal MappingConfigContinuation(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        /// <summary>
        /// Configure how this mapper performs another mapping.
        /// </summary>
        public IFullMappingConfigurator<TSource, TTarget> And
            => new MappingConfigurator<TSource, TTarget>(_configInfo.CloneForContinuation());
    }
}