namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal interface IDataSourceFactory
    {
        IDataSource Create(IChildMemberMappingData mappingData);
    }
}