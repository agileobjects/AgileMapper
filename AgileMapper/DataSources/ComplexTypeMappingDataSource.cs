namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IDataSource complexTypeDataSource,
            int dataSourceIndex,
            IMemberMappingData mappingData)
            : base(
                  complexTypeDataSource.SourceMember,
                  complexTypeDataSource.Variables,
                  GetMapping(complexTypeDataSource, dataSourceIndex, mappingData),
                  complexTypeDataSource.Condition)
        {
        }

        private static Expression GetMapping(
            IDataSource complexTypeDataSource,
            int dataSourceIndex,
            IMemberMappingData mappingData)
        {
            var mapping = MappingFactory.GetChildMapping(
                complexTypeDataSource.SourceMember,
                complexTypeDataSource.Value,
                dataSourceIndex,
                mappingData);

            return mapping;
        }

        public ComplexTypeMappingDataSource(int dataSourceIndex, IMemberMappingData mappingData)
            : base(mappingData.MapperData.SourceMember, GetMapping(dataSourceIndex, mappingData))
        {
        }

        private static Expression GetMapping(int dataSourceIndex, IMemberMappingData mappingData)
            => MappingFactory.GetChildMapping(dataSourceIndex, mappingData);
    }
}