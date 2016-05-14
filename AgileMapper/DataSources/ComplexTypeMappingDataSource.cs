namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(IMemberMappingContext context)
            : base(context.Parent.GetMapCall(context.TargetMember.LeafMember))
        {
        }
    }
}