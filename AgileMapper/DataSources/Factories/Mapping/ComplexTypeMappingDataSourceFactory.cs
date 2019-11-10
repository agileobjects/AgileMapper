namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using ObjectPopulation;

    internal class ComplexTypeMappingDataSourceFactory : IMappingDataSourceFactory
    {
        public bool IsFor(IObjectMappingData mappingData) => true;

        public IDataSource CreateFor(IObjectMappingData mappingData)
            => ComplexTypeDataSource.Create(mappingData);
    }
}