namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.Configuration;
    using ComplexTypes;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using DataSources;
    using Extensions;
    using Extensions.Internal;
    using Members;

    internal static class ConfiguredMappingFactory
    {
        public static Expression GetMappingOrNull(
            MappingCreationContext context,
            out bool isConditional)
        {
            var mappingData = context.MappingData;
            var mapping = GetMappingOrNull(mappingData, out isConditional);

            if ((mapping?.NodeType != ExpressionType.Goto) ||
                (context.PostMappingCallback == null))
            {
                return mapping;
            }

            mapping = ((GotoExpression)mapping).Value;
            
            mapping = TargetObjectResolutionFactory.GetObjectResolution(
                mapping,
                mappingData,
                assignTargetObject: true);

            mapping = mappingData.MapperData.LocalVariable.AssignTo(mapping);

            return mapping;
        }

        private static Expression GetMappingOrNull(
            IObjectMappingData mappingData,
            out bool isConditional)
        {
            var mappingFactoryDataSources =
                GetMappingFactoryDataSources(mappingData.MapperData);

            if (mappingFactoryDataSources.None())
            {
                isConditional = false;
                return null;
            }

            var mappingFactories = DataSourceSet.For(
                mappingFactoryDataSources,
                mappingData,
                ValueExpressionBuilders.ValueSequence);

            isConditional = mappingFactoryDataSources.Last().IsConditional;

            return mappingFactories.BuildValue();
        }

        private static IList<IDataSource> GetMappingFactoryDataSources(ObjectMapperData mapperData)
        {
            return QueryMappingFactories(mapperData)
                .Project(mapperData, GetMappingFactoryDataSource)
                .ToArray();
        }

        public static bool HasMappingFactories(IQualifiedMemberContext context)
            => QueryMappingFactories(context).Any();

        private static IEnumerable<ConfiguredObjectFactory> QueryMappingFactories(IQualifiedMemberContext context)
            => context.MapperContext.UserConfigurations.QueryMappingFactories(context);

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
                factory.ConfigInfo.IsSequentialConfiguration,
                factory.ConfigInfo.HasTargetMemberMatcher(),
                mapperData);
        }
    }
}