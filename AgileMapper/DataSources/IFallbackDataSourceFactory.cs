namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal interface IFallbackDataSourceFactory
    {
        IDataSource Create(IMemberMapperData mapperData);
    }
}