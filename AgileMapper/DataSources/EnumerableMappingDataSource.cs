namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class EnumerableMappingDataSource : DataSourceBase
    {
        public EnumerableMappingDataSource(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IMemberMappingData mappingData)
            : base(
                  sourceEnumerableDataSource.SourceMember,
                  sourceEnumerableDataSource.Variables,
                  GetMapping(sourceEnumerableDataSource, dataSourceIndex, mappingData),
                  sourceEnumerableDataSource.Condition)
        {
        }

        private static Expression GetMapping(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IMemberMappingData mappingData)
        {
            var mapping = InlineMappingFactory.GetChildMapping(
                sourceEnumerableDataSource.SourceMember,
                sourceEnumerableDataSource.Value,
                dataSourceIndex,
                mappingData);

            return mapping;
        }
    }
}