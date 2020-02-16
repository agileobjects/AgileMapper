namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using Members;

    internal static class DefaultValueFallbackDataSourceFactory
    {
        public static IDataSource Create(IMemberMapperData mapperData)
            => new DefaultValueFallbackDataSource(mapperData);

        private class DefaultValueFallbackDataSource : DataSourceBase
        {
            public DefaultValueFallbackDataSource(IQualifiedMemberContext context)
                : base(context.SourceMember, context.GetTargetMemberDefault())
            {
            }

            public override bool IsFallback => true;
        }
    }
}