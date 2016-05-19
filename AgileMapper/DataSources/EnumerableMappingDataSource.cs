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
                  sourceEnumerableDataSource,
                  context.Parent.GetMapCall(
                      sourceEnumerableDataSource.Value,
                      context.TargetMember,
                      dataSourceIndex))
        {
        }
    }
}