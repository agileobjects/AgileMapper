namespace AgileObjects.AgileMapper
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions.Internal;
    using ObjectPopulation;

    internal static class MappingDataExtensions
    {
        public static bool IsStandalone(this IObjectMappingData mappingData)
            => mappingData.IsRoot || mappingData.MappingTypes.RuntimeTypesNeeded;

        public static bool IsTargetConstructable(this IObjectMappingData mappingData)
            => mappingData.GetTargetObjectCreation() != null;

        public static bool IsConstructableFromToTargetDataSource(this IObjectMappingData mappingData)
            => mappingData.GetToTargetDataSourceOrNullForTargetType() != null;

        public static IConfiguredDataSource GetToTargetDataSourceOrNullForTargetType(this IObjectMappingData mappingData)
        {
            var toTargetDataSources = mappingData
                .MapperData
                .MapperContext
                .UserConfigurations
                .GetDataSourcesForToTarget(mappingData.MapperData);

            if (toTargetDataSources.None())
            {
                return null;
            }

            foreach (var dataSource in toTargetDataSources)
            {
                mappingData = mappingData.WithSource(dataSource.SourceMember);

                if (mappingData.IsTargetConstructable())
                {
                    return dataSource;
                }
            }

            return null;
        }

        public static Expression GetTargetObjectCreation(this IObjectMappingData mappingData)
        {
            return mappingData
                .MapperData
                .MapperContext
                .ConstructionFactory
                .GetNewObjectCreation(mappingData);
        }
    }
}
