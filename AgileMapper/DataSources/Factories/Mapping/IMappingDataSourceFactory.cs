namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using ObjectPopulation;

    internal interface IMappingDataSourceFactory
    {
        bool IsFor(IObjectMappingData mappingData);

        IDataSource CreateFor(IObjectMappingData mappingData);
    }
}