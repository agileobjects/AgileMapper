namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using Members;

    internal delegate bool MaptimeDataSourcesFactorySource(
        IMemberMapperData mapperData,
        out MaptimeDataSourceFactory maptimeDataSourceFactory);
}