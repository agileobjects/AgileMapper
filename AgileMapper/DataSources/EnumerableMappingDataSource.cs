namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class EnumerableMappingDataSource : DataSourceBase
    {
        public EnumerableMappingDataSource(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IChildMemberMappingData enumerableMappingData)
            : base(
                  sourceEnumerableDataSource.SourceMember,
                  sourceEnumerableDataSource.Variables,
                  GetMapping(sourceEnumerableDataSource, dataSourceIndex, enumerableMappingData),
                  sourceEnumerableDataSource.Condition)
        {
        }

        private static Expression GetMapping(
            IDataSource sourceEnumerableDataSource,
            int dataSourceIndex,
            IChildMemberMappingData enumerableMappingData)
        {
            var mapping = MappingFactory.GetChildMapping(
                sourceEnumerableDataSource.SourceMember,
                sourceEnumerableDataSource.Value,
                dataSourceIndex,
                enumerableMappingData);

            return mapping;
        }
    }
}