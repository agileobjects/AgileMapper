namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using ObjectPopulation;

    internal interface IMappingRootDataSourceFactory
    {
        bool IsFor(IObjectMappingData mappingData);

        IDataSource CreateFor(IObjectMappingData mappingData);
    }
}