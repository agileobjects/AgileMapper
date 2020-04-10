namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class ConfiguredMappingFactoryDataSourceFactory : IMappingDataSourceFactory
    {
        public bool IsFor(IObjectMappingData mappingData)
            => QueryMappingFactories(mappingData.MapperData).Any();

        public IDataSource CreateFor(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var mappingFactoryDataSources = QueryMappingFactories(mapperData)
                .Project(mapperData, (md, cof) => (IDataSource)new ConfiguredDataSource(
                    md.SourceMember,
                    cof.GetConditionOrNull(md),
                    cof.Create(md),
                    md))
                .ToArray();

            var mappingFactories = DataSourceSet.For(
                mappingFactoryDataSources,
                mapperData,
                ValueExpressionBuilders.ConditionTree);

            return new AdHocDataSource(
                mapperData.SourceMember,
                mappingFactories.BuildValue(),
                null,
                mappingFactories.Variables);
        }

        private static IEnumerable<ConfiguredObjectFactory> QueryMappingFactories(
            IQualifiedMemberContext context)
        {
            return context
                .MapperContext
                .UserConfigurations
                .QueryMappingFactories(context);
        }
    }
}
