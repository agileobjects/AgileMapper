namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using ObjectPopulation;

    internal abstract class MappingDataSourceFactoryBase : IMappingDataSourceFactory
    {
        private readonly MappingExpressionFactoryBase _mappingExpressionFactory;

        protected MappingDataSourceFactoryBase(MappingExpressionFactoryBase mappingExpressionFactory)
        {
            _mappingExpressionFactory = mappingExpressionFactory;
        }

        public abstract bool IsFor(IObjectMappingData mappingData);

        public IDataSource CreateFor(IObjectMappingData mappingData)
        {
            var mappingExpression = _mappingExpressionFactory.Create(mappingData);

            return new AdHocDataSource(
                mappingData.MapperData.SourceMember,
                mappingExpression);
        }
    }
}