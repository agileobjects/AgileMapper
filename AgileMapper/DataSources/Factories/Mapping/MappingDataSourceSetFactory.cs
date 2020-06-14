namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using Extensions.Internal;
    using ObjectPopulation;

    internal static class MappingDataSourceSetFactory
    {
        private static readonly IMappingDataSourceFactory[] _mappingDataSourceFactories =
        {
            new QueryProjectionDataSourceFactory(),
            new RootEnumMappingDataSourceFactory(),
            new DictionaryMappingDataSourceFactory(),
            new EnumerableMappingDataSourceFactory(),
            new ComplexTypeMappingDataSourceFactory()
        };

        public static IDataSourceSet CreateFor(IObjectMappingData mappingData)
        {
            var dataSourceFactory = _mappingDataSourceFactories
                .First(mappingData, (md, dsf) => dsf.IsFor(md));

            var dataSource = dataSourceFactory.CreateFor(mappingData);

            return DataSourceSet.For(dataSource, mappingData);
        }
    }
}