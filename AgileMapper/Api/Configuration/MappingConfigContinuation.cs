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

        private UserConfigurationSet UserConfigurations => _configInfo.MapperContext.UserConfigurations;

        IMappingConfigContinuation<TSource, TTarget> ICustomDataSourceMappingConfigContinuation<TSource, TTarget>.AndViceVersa()
        {
            if (UserConfigurations.ReverseConfigurationSources == false)
            {
                UserConfigurations.AddReverseOf(_configInfo);
                return this;
            }

            throw new MappingConfigurationException(
                $"'{GetDataSourceDescription()}' does not need to be explitly reversed, " +
                "because data source reversal is enabled by default");
        }

        IMappingConfigContinuation<TSource, TTarget> ICustomDataSourceMappingConfigContinuation<TSource, TTarget>.ButNotViceVersa()
        {
            if (UserConfigurations.ReverseConfigurationSources)
            {
                UserConfigurations.RemoveReverseOf(_configInfo);
                return this;
            }

            throw new MappingConfigurationException(
                $"'{GetDataSourceDescription()}' does not need to have its reverse suppressed, " +
                 "because data source reversal is disabled by default");
        }

        private string GetDataSourceDescription()
            => UserConfigurations.GetDataSourceFactoryFor(_configInfo).GetDescription();

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