namespace AgileObjects.AgileMapper.Configuration.DataSources
{
    internal interface IReversibleConfiguredDataSourceFactory
    {
        MappingConfigInfo ConfigInfo { get; }

        ConfiguredDataSourceFactoryBase CreateReverseIfAppropriate(bool isAutoReversal);
    }
}