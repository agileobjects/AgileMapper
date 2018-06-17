namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using DataSources;
    using Members;

    internal struct DefaultValueDataSourceFactory : IDataSourceFactory
    {
        public IDataSource Create(IMemberMapperData mapperData)
            => new DefaultValueDataSource(mapperData);

        private class DefaultValueDataSource : DataSourceBase
        {
            public DefaultValueDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, mapperData.GetTargetMemberDefault())
            {
            }
        }
    }
}