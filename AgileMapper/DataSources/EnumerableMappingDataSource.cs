namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class EnumerableMappingDataSource : DataSourceBase
    {
        public EnumerableMappingDataSource(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            MemberMapperData mapperData)
            : base(
                  sourceEnumerableDataSource.SourceMember,
                  sourceEnumerableDataSource.Variables,
                  GetMapCall(sourceEnumerableDataSource.Value, dataSourceIndex, mapperData),
                  sourceEnumerableDataSource.Condition)
        {
        }

        private static Expression GetMapCall(Expression value, int dataSourceIndex, MemberMapperData mapperData)
        {
            return mapperData.GetMapCall(value, dataSourceIndex);
        }
    }
}