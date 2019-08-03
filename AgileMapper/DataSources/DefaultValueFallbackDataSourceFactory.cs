namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal struct DefaultValueFallbackDataSourceFactory : IFallbackDataSourceFactory
    {
        public IDataSource Create(IMemberMapperData mapperData)
            => new DefaultValueDataSource(mapperData);

        private class DefaultValueDataSource : DataSourceBase
        {
            public DefaultValueDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, mapperData.GetTargetMemberDefault())
            {
            }

            public override bool IsFallback => true;
        }
    }
}