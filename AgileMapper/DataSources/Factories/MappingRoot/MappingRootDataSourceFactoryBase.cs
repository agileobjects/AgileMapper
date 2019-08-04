namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using ObjectPopulation;

    internal abstract class MappingRootDataSourceFactoryBase
    {
        private readonly MappingExpressionFactoryBase _mappingExpressionFactory;

        protected MappingRootDataSourceFactoryBase(MappingExpressionFactoryBase mappingExpressionFactory)
        {
            _mappingExpressionFactory = mappingExpressionFactory;
        }

        public IDataSource CreateFor(IObjectMappingData mappingData)
        {
            var mappingExpression = _mappingExpressionFactory.Create(mappingData);

            return new AdHocDataSource(
                mappingData.MapperData.SourceMember,
                mappingExpression);
        }
    }
}