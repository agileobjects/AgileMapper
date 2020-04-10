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
            => QueryObjectFactories(mappingData.MapperData).Any();

        public IDataSource CreateFor(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var mappingFactoryDataSources = QueryObjectFactories(mapperData)
                .Project(mapperData, (md, cof) => (IDataSource)new ConfiguredDataSource(
                    md.SourceMember,
                    cof.GetConditionOrNull(md),
                    cof.Create(md),
                    md))
                .ToArray();

            var mappingFactories = DataSourceSet.For(
                mappingFactoryDataSources,
                mapperData,
                ValueExpressionBuilders.ValueSequence);

            return new AdHocDataSource(
                mapperData.SourceMember,
                mappingFactories.BuildValue(),
                null,
                mappingFactories.Variables);
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
