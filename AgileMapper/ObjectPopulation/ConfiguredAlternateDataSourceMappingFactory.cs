namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using DataSources;
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class ConfiguredAlternateDataSourceMappingFactory
    {
        public static Expression GetMappingOrNull(
            IObjectMappingData mappingData,
            out bool isConditional)
        {
            var toTargetDataSources = mappingData
                .GetToTargetDataSources(sequential: false);

            IDataSourceSet toTargetDataSourcesSet;
            IConfiguredDataSource toTargetDataSource;

            switch (toTargetDataSources.Count)
            {
                case 0:
                    isConditional = false;
                    return null;

                case 1:
                    toTargetDataSource = toTargetDataSources[0];
                    var toTargetMappingData = mappingData.WithToTargetSource(toTargetDataSource.SourceMember);
                    toTargetDataSourcesSet = DataSourceSet.For(toTargetDataSource, toTargetMappingData);
                    break;

                default:
                    toTargetDataSourcesSet = DataSourceSet.For(
                        toTargetDataSources.ProjectToArray(cds => (IDataSource)cds),
                        mappingData,
                        ValueExpressionBuilders.ValueSequence);
                    
                    toTargetDataSource = toTargetDataSources.Last();
                    break;
            }

            isConditional = toTargetDataSource.HasConfiguredCondition;
            return toTargetDataSourcesSet.BuildValue();
        }
    }
}
