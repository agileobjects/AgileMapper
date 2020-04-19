namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    internal interface IConfiguredDataSourceFactoryFactory
    {
        ConfiguredDataSourceFactory CreateFromLambda<TTargetValue>();

        ConfiguredDataSourceFactory CreateForCtorParam();

        ConfiguredDataSourceFactory CreateForCtorParam<TTargetParam>();

        ConfiguredDataSourceFactory CreateForToTarget();
    }
}