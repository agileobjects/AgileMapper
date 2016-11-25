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
        /// Perform another configuration of how this mapper maps to and from the source and target types
        /// being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        public IFullMappingConfigurator<TSource, TTarget> And
            => new MappingConfigurator<TSource, TTarget>(_configInfo.CloneForContinuation());

        /// <summary>
        /// Perform an alternative configuration of how this mapper maps to and from the source and target types
        /// being configured. This property exists purely to provide a more fluent configuration interface.
        /// </summary>
        public IFullMappingConfigurator<TSource, TTarget> But
            => new MappingConfigurator<TSource, TTarget>(_configInfo.CloneForContinuation());
    }
}