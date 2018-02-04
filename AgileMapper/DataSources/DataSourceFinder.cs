﻿namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using Members;

    internal class DataSourceFinder
    {
        public static readonly DataSourceFinder Instance = new DataSourceFinder();

        private readonly ICollection<IMaptimeDataSourceFactory> _mapTimeDataSourceFactories;

        private DataSourceFinder()
        {
            _mapTimeDataSourceFactories = new List<IMaptimeDataSourceFactory>
            {
                new DictionaryDataSourceFactory()
            };
        }

        public static DataSourceSet FindDataSources(IChildMemberMappingData childMappingData)
            => childMappingData.MapperData.MapperContext.DataSources.FindFor(childMappingData);

        public DataSourceSet FindFor(IChildMemberMappingData childMappingData)
        {
            var validDataSources = EnumerateDataSources(childMappingData)
                .Where(ds => ds.IsValid)
                .ToArray();

            return new DataSourceSet(childMappingData.MapperData, validDataSources);
        }

        private IEnumerable<IDataSource> EnumerateDataSources(IChildMemberMappingData childMappingData)
        {
            var dataSourceIndex = 0;

            if (DataSourcesAreConfigured(childMappingData.MapperData, out var configuredDataSources))
            {
                foreach (var configuredDataSource in configuredDataSources)
                {
                    yield return GetFinalDataSource(configuredDataSource, dataSourceIndex, childMappingData);

                    if (!configuredDataSource.IsConditional)
                    {
                        yield break;
                    }

                    ++dataSourceIndex;
                }
            }

            if (UseMaptimeDataSources(childMappingData, out var maptimeDataSources))
            {
                foreach (var maptimeDataSource in maptimeDataSources)
                {
                    yield return GetFinalDataSource(maptimeDataSource, dataSourceIndex, childMappingData);

                    if (!maptimeDataSource.IsConditional)
                    {
                        yield break;
                    }
                }

                yield return GetFallbackDataSourceFor(childMappingData);
                yield break;
            }

            var sourceMemberDataSources =
                GetSourceMemberDataSources(configuredDataSources, dataSourceIndex, childMappingData);

            foreach (var dataSource in sourceMemberDataSources)
            {
                yield return dataSource;
            }
        }

        private static bool DataSourcesAreConfigured(
            IMemberMapperData mapperData,
            out IList<IConfiguredDataSource> configuredDataSources)
        {
            configuredDataSources = mapperData
                .MapperContext
                .UserConfigurations
                .GetDataSources(mapperData);

            return configuredDataSources.Any();
        }

        private bool UseMaptimeDataSources(
            IChildMemberMappingData childMappingData,
            out IEnumerable<IDataSource> maptimeDataSources)
        {
            var applicableFactory = _mapTimeDataSourceFactories
                .FirstOrDefault(factory => factory.IsFor(childMappingData.MapperData));

            if (applicableFactory == null)
            {
                maptimeDataSources = Enumerable<IDataSource>.Empty;
                return false;
            }

            maptimeDataSources = applicableFactory.Create(childMappingData);
            return true;
        }

        private static IEnumerable<IDataSource> GetSourceMemberDataSources(
            IList<IConfiguredDataSource> configuredDataSources,
            int dataSourceIndex,
            IChildMemberMappingData mappingData)
        {
            if (mappingData.MapperData.TargetMember.IsCustom)
            {
                yield break;
            }

            var bestMatchingSourceMember = SourceMemberMatcher.GetMatchFor(mappingData, out var contextMappingData);
            var matchingSourceMemberDataSource = GetSourceMemberDataSourceOrNull(bestMatchingSourceMember, contextMappingData);

            if ((matchingSourceMemberDataSource == null) ||
                 configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (dataSourceIndex == 0)
                {
                    if (mappingData.MapperData.TargetMember.IsComplex &&
                       (mappingData.MapperData.TargetMember.Type != typeof(object)))
                    {
                        yield return new ComplexTypeMappingDataSource(dataSourceIndex, mappingData);
                    }
                }
                else if (configuredDataSources.Any() && configuredDataSources.Last().IsConditional)
                {
                    yield return GetFallbackDataSourceFor(mappingData);
                }

                yield break;
            }

            yield return matchingSourceMemberDataSource;

            if (mappingData.MapperData.TargetMember.IsReadOnly)
            {
                yield break;
            }

            if (matchingSourceMemberDataSource.IsConditional &&
               (matchingSourceMemberDataSource.IsValid || configuredDataSources.Any()))
            {
                yield return GetFallbackDataSourceFor(mappingData);
            }
        }

        private static IDataSource GetSourceMemberDataSourceOrNull(
            IQualifiedMember bestMatchingSourceMember,
            IChildMemberMappingData mappingData)
        {
            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            var sourceMemberDataSource = SourceMemberDataSource
                .For(bestMatchingSourceMember, mappingData.MapperData);

            return GetFinalDataSource(sourceMemberDataSource, 0, mappingData);
        }

        private static IDataSource GetFallbackDataSourceFor(IChildMemberMappingData mappingData)
            => mappingData.RuleSet.FallbackDataSourceFactory.Create(mappingData.MapperData);

        private static IDataSource GetFinalDataSource(
            IDataSource foundDataSource,
            int dataSourceIndex,
            IChildMemberMappingData childMappingData)
        {
            var childTargetMember = childMappingData.MapperData.TargetMember;

            if (UseComplexTypeDataSource(foundDataSource, childTargetMember))
            {
                return new ComplexTypeMappingDataSource(foundDataSource, dataSourceIndex, childMappingData);
            }

            if (childTargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(foundDataSource, dataSourceIndex, childMappingData);
            }

            return foundDataSource;
        }

        private static bool UseComplexTypeDataSource(IDataSource dataSource, QualifiedMember targetMember)
        {
            if (!targetMember.IsComplex)
            {
                return false;
            }

            if (targetMember.IsDictionary)
            {
                return true;
            }

            if (targetMember.Type == typeof(object))
            {
                return !dataSource.SourceMember.Type.IsSimple();
            }

            return !targetMember.Type.IsFromBcl();
        }
    }
}