namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;
    using Projection;

    internal class MappingConfigContinuation<TSource, TTarget> :
        ICustomDataSourceMappingConfigContinuation<TSource, TTarget>,
        IProjectionConfigContinuation<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        public MappingConfigContinuation(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        IMappingConfigContinuation<TSource, TTarget> ICustomDataSourceMappingConfigContinuation<TSource, TTarget>.AndViceVersa()
        {
            return this;
        }

        IMappingConfigContinuation<TSource, TTarget> ICustomDataSourceMappingConfigContinuation<TSource, TTarget>.ButNotViceVersa()
        {
            _configInfo.MapperContext.UserConfigurations.RemoveReverseOf(_configInfo);
            return this;
        }

        public IFullMappingConfigurator<TSource, TTarget> And => CreateNewConfigurator();

        IFullProjectionConfigurator<TSource, TTarget> IProjectionConfigContinuation<TSource, TTarget>.And
            => CreateNewConfigurator();

        public IFullMappingConfigurator<TSource, TTarget> But => CreateNewConfigurator();

        IFullProjectionConfigurator<TSource, TTarget> IProjectionConfigContinuation<TSource, TTarget>.But
            => CreateNewConfigurator();

        private MappingConfigurator<TSource, TTarget> CreateNewConfigurator()
            => new MappingConfigurator<TSource, TTarget>(_configInfo.Copy());
    }
}