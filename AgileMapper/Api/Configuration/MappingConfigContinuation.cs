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
            var dataSourceFactory = UserConfigurations.GetDataSourceFactoryFor(_configInfo);

            if (dataSourceFactory.CannotBeReversed(out var reason))
            {
                throw new MappingConfigurationException(
                    $"'{GetDataSourceDescription()}' cannot be reversed, because {reason}");
            }

            if (UserConfigurations.AutoDataSourceReversalEnabled(dataSourceFactory) == false)
            {
                UserConfigurations.AddReverseDataSourceFor(dataSourceFactory);
                return this;
            }

            throw new MappingConfigurationException(
                $"'{GetDataSourceDescription()}' does not need to be explicitly reversed, " +
                "because configured data source reversal is enabled by default");
        }

        IMappingConfigContinuation<TSource, TTarget> ICustomDataSourceMappingConfigContinuation<TSource, TTarget>.ButNotViceVersa()
        {
            if (UserConfigurations.AutoDataSourceReversalEnabled(_configInfo))
            {
                UserConfigurations.RemoveReverseOf(_configInfo);
                return this;
            }

            throw new MappingConfigurationException(
                $"'{GetDataSourceDescription()}' does not need to have its reverse suppressed, " +
                 "because configured data source reversal is disabled by default");
        }

        private string GetDataSourceDescription()
            => UserConfigurations.GetDataSourceFactoryFor(_configInfo).GetDescription();

        public MappingConfigStartingPoint AndWhenMapping
            => new MappingConfigStartingPoint(_configInfo.MapperContext);

        public IFullMappingConfigurator<TSource, TTarget> And => CreateNewConfigurator();

        IFullProjectionConfigurator<TSource, TTarget> IProjectionConfigContinuation<TSource, TTarget>.And
            => CreateNewConfigurator();

        public IFullMappingConfigurator<TSource, TTarget> But => CreateNewConfigurator();

        IFullProjectionConfigurator<TSource, TTarget> IProjectionConfigContinuation<TSource, TTarget>.But
            => CreateNewConfigurator();

        public IFullMappingConfigurator<TSource, TTarget> Then
            => CreateNewConfigurator(CopyConfigInfo().ForSequentialConfiguration());

        private MappingConfigurator<TSource, TTarget> CreateNewConfigurator()
            => CreateNewConfigurator(CopyConfigInfo());

        private MappingConfigInfo CopyConfigInfo() => _configInfo.Copy();

        private static MappingConfigurator<TSource, TTarget> CreateNewConfigurator(
            MappingConfigInfo configInfo)
        {
            return new MappingConfigurator<TSource, TTarget>(configInfo);
        }
    }
}