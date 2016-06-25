namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class EnumerableMappingDataSource : DataSourceBase
    {
        public EnumerableMappingDataSource(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IMemberMappingContext context)
            : base(
                  sourceEnumerableDataSource.SourceMember,
                  sourceEnumerableDataSource.NestedAccesses,
                  sourceEnumerableDataSource.Variables,
                  context.GetMapCall(sourceEnumerableDataSource.Value, dataSourceIndex))
        {
        }
    }
}