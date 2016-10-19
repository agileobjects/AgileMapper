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
                  GetMapping(sourceEnumerableDataSource, dataSourceIndex, mapperData),
                  sourceEnumerableDataSource.Condition)
        {
        }

        private static Expression GetMapping(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            var mapping = InlineMappingFactory.GetChildMapping(
                sourceEnumerableDataSource.SourceMember,
                sourceEnumerableDataSource.Value,
                dataSourceIndex,
                mapperData);

            return mapping;
        }
    }
}