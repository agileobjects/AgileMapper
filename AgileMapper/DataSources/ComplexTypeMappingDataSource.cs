namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IQualifiedMember sourceMember,
            IMemberMappingContext context,
            int dataSourceIndex)
            : base(
                  sourceMember,
                  context.Parent.GetMapCall(
                      context.SourceObject,
                      context.TargetMember,
                      dataSourceIndex))
        {
        }
    }
}