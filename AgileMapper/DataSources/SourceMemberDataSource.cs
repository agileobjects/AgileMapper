namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class SourceMemberDataSource : DataSourceBase
    {
        public SourceMemberDataSource(QualifiedMember sourceMember, IMemberMappingContext context)
            : base(sourceMember.GetAccess(context.SourceObject), context)
        {
        }
    }
}