namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(int dataSourceIndex, IMemberMappingContext context)
            : this(
                  context.MapperContext.DataSources.GetSourceMemberFor(context) ?? context.SourceMember,
                  dataSourceIndex,
                  context)
        {
        }

        private ComplexTypeMappingDataSource(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMappingContext context)
            : base(sourceMember, GetMapCall(sourceMember, dataSourceIndex, context))
        {
        }

        private static Expression GetMapCall(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMappingContext context)
        {
            var relativeMember = sourceMember.RelativeTo(context.SourceMember);
            var relativeMemberAccess = relativeMember.GetQualifiedAccess(context.SourceObject);

            return context.GetMapCall(relativeMemberAccess, dataSourceIndex);
        }
    }
}