﻿namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration.DataSources;

    internal interface IConfiguredDataSourceFactoryFactory
    {
        ConfiguredDataSourceFactory CreateFromLambda<TTargetValue>();

        ConfiguredDataSourceFactory CreateForCtorParam();

        ConfiguredDataSourceFactory CreateForCtorParam<TTargetParam>();

        ConfiguredDataSourceFactory CreateForToTarget(bool isSequential);
    }
}