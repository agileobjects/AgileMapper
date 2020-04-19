namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    internal static class ConfigInfoDataSourceExtensions
    {
        public static ISequencedDataSourceFactory[] GetSequenceDataSourceFactories(
            this MappingConfigInfo configInfo)
        {
            return configInfo.Get<ISequencedDataSourceFactory[]>();
        }

        public static MappingConfigInfo ForSequentialConfiguration(
            this MappingConfigInfo configInfo,
            ISequencedDataSourceFactory[] dataSourceFactorySequence)
        {
            return configInfo.Copy()
                .ForSequentialConfiguration()
                .SetSequenceDataSourceFactories(dataSourceFactorySequence);
        }

        public static MappingConfigInfo SetSequenceDataSourceFactories(
            this MappingConfigInfo configInfo,
            ISequencedDataSourceFactory[] dataSourceFactorySequence)
        {
            return configInfo.Set(dataSourceFactorySequence);
        }
    }
}