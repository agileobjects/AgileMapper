namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using ObjectPopulation;

    internal class ComplexTypeMappingRootDataSourceFactory : IMappingRootDataSourceFactory
    {
        public bool IsFor(IObjectMappingData mappingData) => true;

        public IDataSource CreateFor(IObjectMappingData mappingData)
            => ComplexTypeDataSource.Create(mappingData);
    }
}