namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class EnumerableMappingDataSource : DataSourceBase
    {
        public EnumerableMappingDataSource(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IMemberMapperData mapperData)
            : base(
                  sourceEnumerableDataSource.SourceMember,
                  sourceEnumerableDataSource.Variables,
                  mapperData.GetMapCall(sourceEnumerableDataSource.Value, dataSourceIndex),
                  sourceEnumerableDataSource.Condition)
        {
        }
    }
}