namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using DataSources;
    using Extensions;
    using Extensions.Internal;

    internal static class ConfiguredMappingFactory
    {
        public static Expression GetMappingOrNull(
            IObjectMappingData mappingData,
            out bool isConditional)
        {
            var mapperData = mappingData.MapperData;

            var mappingFactoryDataSources = GetMappingFactoryDataSources(mapperData);

            if (mappingFactoryDataSources.None())
            {
                isConditional = false;
                return null;
            }

            var mappingFactories = DataSourceSet.For(
                mappingFactoryDataSources,
                mapperData,
                ValueExpressionBuilders.ValueSequence);

            isConditional = mappingFactoryDataSources.Last().IsConditional;

            return mappingFactories.BuildValue();
        }

        private static IList<IDataSource> GetMappingFactoryDataSources(ObjectMapperData mapperData)
        {
            return mapperData
                .MapperContext
                .UserConfigurations
                .QueryMappingFactories(mapperData)
                .Project(mapperData, GetMappingFactoryDataSource)
                .ToArray();
        }

        private static IDataSource GetMappingFactoryDataSource(
            ObjectMapperData mapperData,
            ConfiguredObjectFactory factory)
        {
            var condition = factory.GetConditionOrNull(mapperData);
            var value = factory.Create(mapperData);
            var returnLabelMapperData = mapperData.OriginalMapperData ?? mapperData;
            var returnValue = returnLabelMapperData.GetReturnExpression(value);

            return new ConfiguredDataSource(
                mapperData.SourceMember,
                condition,
                returnValue,
                mapperData);
        }
    }
}