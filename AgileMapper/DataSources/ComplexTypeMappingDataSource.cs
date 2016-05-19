namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMappingContext context)
            : base(sourceMember, GetMapCall(context.SourceObject, dataSourceIndex, context))
        {
        }

        public static Expression GetMapCall(Expression value, int dataSourceIndex, IMemberMappingContext context)
            => context.Parent.GetMapCall(value, context.TargetMember, dataSourceIndex);
    }
}