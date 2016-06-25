namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMappingContext context)
            : base(sourceMember, context.GetMapCall(context.SourceObject, dataSourceIndex))
        {
        }
    }
}