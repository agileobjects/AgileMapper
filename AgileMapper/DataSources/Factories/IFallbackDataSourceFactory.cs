namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using Members;

    internal interface IFallbackDataSourceFactory
    {
        IDataSource Create(IMemberMapperData mapperData);
    }
}