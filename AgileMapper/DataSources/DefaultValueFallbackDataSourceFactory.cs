namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal struct DefaultValueFallbackDataSourceFactory : IFallbackDataSourceFactory
    {
        public IDataSource Create(IMemberMapperData mapperData)
            => new DefaultValueFallbackDataSource(mapperData);

        private class DefaultValueFallbackDataSource : DataSourceBase
        {
            public DefaultValueFallbackDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, mapperData.GetTargetMemberDefault())
            {
            }

            public override bool IsFallback => true;
        }
    }
}