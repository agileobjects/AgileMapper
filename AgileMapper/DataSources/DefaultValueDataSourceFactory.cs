namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal struct DefaultValueDataSourceFactory : IDataSourceFactory
    {
        public IDataSource Create(IMemberMapperData mapperData)
            => new DefaultValueDataSource(mapperData);
    }
}