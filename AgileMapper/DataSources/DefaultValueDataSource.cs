namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class DefaultValueDataSource : DataSourceBase
    {
        public DefaultValueDataSource(IMemberMapperData mapperData)
            : base(mapperData.SourceMember, mapperData.GetTargetMemberDefault())
        {
        }
    }
}