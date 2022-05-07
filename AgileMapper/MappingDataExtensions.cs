namespace AgileObjects.AgileMapper;

using System.Collections.Generic;
using System.Diagnostics;
using DataSources;
using Extensions.Internal;
using Members;
using ObjectPopulation;
using ObjectPopulation.ComplexTypes;

internal static class MappingDataExtensions
{
    [DebuggerStepThrough]
    public static bool IsStandalone(this IObjectMappingData mappingData)
        => mappingData.IsRoot || mappingData.MappingTypes.RuntimeTypesNeeded;

    public static bool IsTargetConstructable(this IObjectMappingData mappingData)
        => GetTargetObjectCreationInfos(mappingData).Any();

    public static IList<IBasicConstructionInfo> GetTargetObjectCreationInfos(this IObjectMappingData mappingData)
    {
        return mappingData
            .MapperData
            .MapperContext
            .ConstructionFactory
            .GetTargetObjectCreationInfos(mappingData);
    }

    public static bool IsConstructableFromToTargetDataSource(this IObjectMappingData mappingData)
        => mappingData.GetToTargetDataSourceOrNullForTargetType() != null;

    public static IConfiguredDataSource GetToTargetDataSourceOrNullForTargetType(this IObjectMappingData mappingData)
    {
        var toTargetDataSources = mappingData.GetToTargetDataSources();

        if (toTargetDataSources.None())
        {
            return null;
        }

        foreach (var dataSource in toTargetDataSources)
        {
            mappingData = mappingData.WithToTargetSource(dataSource.SourceMember);

            if (mappingData.IsTargetConstructable())
            {
                return dataSource;
            }
        }

        // TODO: Cover: Unconstructable ToTarget data source
        return null;
    }

    public static IList<IConfiguredDataSource> GetToTargetDataSources(this IObjectMappingData mappingData)
        => mappingData.MapperData.GetToTargetDataSources();

    public static IList<IConfiguredDataSource> GetToTargetDataSources(
        this IMemberMapperData mapperData,
        bool? sequential = null)
    {
        return mapperData
            .MapperContext
            .UserConfigurations
            .GetDataSourcesForToTarget(mapperData, sequential);
    }

    public static bool HasSameTypedConfiguredDataSource(this IObjectMappingData mappingData)
    {
        return
            (mappingData.MapperData.SourceType == mappingData.MapperData.TargetType) &&
            (mappingData.MapperData.SourceMember is ConfiguredSourceMember);
    }

    public static IMappingData<TSource, TTarget> ToTyped<TSource, TTarget>(
        this IMappingData mappingData)
    {
        if (mappingData is IMappingData<TSource, TTarget> typedMappingData)
        {
            return typedMappingData;
        }

        return new MappingInstanceData<TSource, TTarget>(mappingData);
    }
}