namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using Extensions.Internal;
    using MappingRoot;
    using ObjectPopulation;

    internal static class RootDataSourceSetFactory
    {
        private static readonly IMappingRootDataSourceFactory[] _mappingRootDataSourceFactories =
        {
            new QueryProjectionRootDataSourceFactory(),
            new EnumMappingRootDataSourceFactory(),
            new DictionaryMappingRootDataSourceFactory(),
            new EnumerableMappingRootDataSourceFactory(),
            new ComplexTypeMappingRootDataSourceFactory()
        };

        public static IDataSourceSet CreateFor(IObjectMappingData rootMappingData)
        {
            var rootDataSourceFactory = _mappingRootDataSourceFactories
                .First(rootMappingData, (rmd, mef) => mef.IsFor(rmd));

            var rootDataSource = rootDataSourceFactory.CreateFor(rootMappingData);

            return DataSourceSet.For(rootDataSource, rootMappingData.MapperData);
        }
    }
}