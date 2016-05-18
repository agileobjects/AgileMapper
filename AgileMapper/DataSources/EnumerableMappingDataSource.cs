namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class EnumerableMappingDataSource : DataSourceBase
    {
        public EnumerableMappingDataSource(
            IDataSource sourceEnumerableDataSource,
            IMemberMappingContext context,
            int dataSourceIndex)
            : base(
                  sourceEnumerableDataSource,
                  context.Parent.GetMapCall(
                      sourceEnumerableDataSource.Value,
                      context.TargetMember,
                      dataSourceIndex))
        {
        }
    }
}