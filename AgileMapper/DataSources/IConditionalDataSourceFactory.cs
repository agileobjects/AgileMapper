namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal interface IConditionalDataSourceFactory : IDataSourceFactory
    {
        bool IsFor(IMemberMapperData mapperData);
    }
}