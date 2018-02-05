namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;
    using Projection;


    internal class MappingConfigContinuation<TSource, TTarget> :
        IMappingConfigContinuation<TSource, TTarget>,
        IProjectionConfigContinuation<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        public MappingConfigContinuation(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public IFullMappingConfigurator<TSource, TTarget> And => CreateNewConfigurator();

        IFullProjectionConfigurator<TSource, TTarget> IProjectionConfigContinuation<TSource, TTarget>.And
            => CreateNewConfigurator();

        public IFullMappingConfigurator<TSource, TTarget> But => CreateNewConfigurator();

        private MappingConfigurator<TSource, TTarget> CreateNewConfigurator()
            => new MappingConfigurator<TSource, TTarget>(_configInfo.Clone());
    }
}