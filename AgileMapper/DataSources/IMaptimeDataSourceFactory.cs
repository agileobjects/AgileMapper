namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal interface IMaptimeDataSourceFactory
    {
        bool IsFor(IMemberMapperData mapperData);

        IDataSource Create(IChildMemberMappingData mappingData);
    }
}