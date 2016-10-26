namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IDataSource complexTypeDataSource,
            int dataSourceIndex,
            IMemberMapperData mapperData)
            : base(
                  complexTypeDataSource.SourceMember,
                  complexTypeDataSource.Variables,
                  GetMapping(complexTypeDataSource, dataSourceIndex, mapperData),
                  complexTypeDataSource.Condition)
        {
        }

        private static Expression GetMapping(
            IDataSource complexTypeDataSource,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            var mapping = InlineMappingFactory.GetChildMapping(
                complexTypeDataSource.SourceMember,
                complexTypeDataSource.Value,
                dataSourceIndex,
                mapperData);

            return mapping;
        }

        public ComplexTypeMappingDataSource(int dataSourceIndex, IMemberMapperData mapperData)
            : base(mapperData.SourceMember, GetMapping(dataSourceIndex, mapperData))
        {
        }

        private static Expression GetMapping(int dataSourceIndex, IMemberMapperData mapperData)
        {
            var mapping = InlineMappingFactory.GetChildMapping(dataSourceIndex, mapperData);
            return mapping;
        }
    }
}