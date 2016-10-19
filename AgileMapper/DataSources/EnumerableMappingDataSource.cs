namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
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
                  GetMapCall(sourceEnumerableDataSource.Value, dataSourceIndex, mapperData),
                  sourceEnumerableDataSource.Condition)
        {
        }

        private static Expression GetMapCall(Expression sourceEnumerable, int dataSourceIndex, IMemberMapperData mapperData)
        {
            return mapperData.GetMapCall(sourceEnumerable, dataSourceIndex);
        }
    }
}