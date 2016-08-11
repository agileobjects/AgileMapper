namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class EnumerableMappingDataSource : DataSourceBase
    {
        public EnumerableMappingDataSource(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            MemberMapperData data)
            : base(
                  sourceEnumerableDataSource.SourceMember,
                  sourceEnumerableDataSource.Variables,
                  GetMapCall(sourceEnumerableDataSource.Value, dataSourceIndex, data),
                  sourceEnumerableDataSource.Condition)
        {
        }

        private static Expression GetMapCall(Expression value, int dataSourceIndex, MemberMapperData data)
        {
            return data.GetMapCall(value, dataSourceIndex);
        }
    }
}