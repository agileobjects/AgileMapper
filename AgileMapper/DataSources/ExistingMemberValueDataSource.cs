namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class ExistingMemberValueDataSource : DataSourceBase
    {
        public ExistingMemberValueDataSource(IMemberMappingContext context)
            : base(
                  context.SourceMember,
                  context.TargetMember.GetAccess(context.InstanceVariable),
                  context)
        {
        }
    }
}