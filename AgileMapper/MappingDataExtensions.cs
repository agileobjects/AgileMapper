namespace AgileObjects.AgileMapper
{
    using System.Linq;
    using DataSources;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;

    internal static class MappingDataExtensions
    {
        public static bool IsStandalone(this IObjectMappingData mappingData)
            => mappingData.IsRoot || mappingData.MappingTypes.RuntimeTypesNeeded;

        public static bool IsTargetConstructable(this IObjectMappingData mappingData)
        {
            return mappingData
                .MapperData
                .MapperContext
                .ConstructionFactory
                .GetTargetObjectCreationInfos(mappingData)
                .Any();
        }

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

            // TODO: Cover: Unconstructable ToTarget data source
            return null;
        }

        public static bool HasSameTypedConfiguredDataSource(this IObjectMappingData mappingData)
        {
            return
                (mappingData.MapperData.SourceType == mappingData.MapperData.TargetType) &&
                (mappingData.MapperData.SourceMember is ConfiguredSourceMember);
        }
    }
}
