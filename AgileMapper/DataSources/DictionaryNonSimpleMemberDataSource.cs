namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class DictionaryNonSimpleMemberDataSource : DataSourceBase
    {
        public DictionaryNonSimpleMemberDataSource(IQualifiedMember sourceMember, IMemberMapperData mapperData)
            : base(sourceMember, sourceMember.GetQualifiedAccess(mapperData))
        {
        }
    }
}