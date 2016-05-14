namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class EnumerableMappingDataSource : DataSourceBase
    {
        public EnumerableMappingDataSource(IDataSource sourceEnumerableDataSource, IMemberMappingContext context)
            : base(
                context.Parent.GetMapCall(sourceEnumerableDataSource.Value, context.TargetMember.LeafMember),
                sourceEnumerableDataSource.NestedAccesses)
        {
        }
    }
}