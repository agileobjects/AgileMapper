namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal struct ExistingOrDefaultValueDataSourceFactory : IDataSourceFactory
    {
        public IDataSource Create(IMemberMapperData mapperData)
            => new ExistingMemberValueOrDefaultDataSource(mapperData);
    }
}