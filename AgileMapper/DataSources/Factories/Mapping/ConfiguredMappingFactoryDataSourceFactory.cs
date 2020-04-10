namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Members;
    using ObjectPopulation;

    internal class ConfiguredMappingFactoryDataSourceFactory : IMappingDataSourceFactory
    {
        public bool IsFor(IObjectMappingData mappingData)
            => QueryObjectFactories(mappingData.MapperData).Any();

        public IDataSource CreateFor(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var mappingFactories = QueryObjectFactories(mapperData).ToArray();

            return null;
        }

        private static IEnumerable<ConfiguredObjectFactory> QueryObjectFactories(
            IQualifiedMemberContext context)
        {
            return context
                .MapperContext
                .UserConfigurations
                .QueryObjectFactories(context);
        }
    }
}
