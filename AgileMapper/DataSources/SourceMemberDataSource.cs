namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class SourceMemberDataSource : DataSourceBase
    {
        public SourceMemberDataSource(IQualifiedMember sourceMember, IMemberMappingContext context)
            : base(sourceMember, sourceMember.GetQualifiedAccess(context.SourceObject), context)
        {
        }
    }
}