namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Members;

    internal class DataSourceFinder
    {
        private readonly ICollection<IMaptimeDataSourceFactory> _mapTimeDataSourceFactories;

        public DataSourceFinder()
        {
            _mapTimeDataSourceFactories = new List<IMaptimeDataSourceFactory>
            {
                new DictionaryDataSourceFactory()
            };
        }

        public DataSourceSet FindFor(IChildMemberMappingData childMappingData)
        {
            var validDataSources = EnumerateDataSources(childMappingData)
                .Where(ds => ds.IsValid)
                .ToArray();

            return new DataSourceSet(validDataSources);
        }

        private IEnumerable<IDataSource> EnumerateDataSources(IChildMemberMappingData childMappingData)
        {
            var dataSourceIndex = 0;

            IEnumerable<IConfiguredDataSource> configuredDataSources;

            if (DataSourcesAreConfigured(childMappingData.MapperData, out configuredDataSources))
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

            IEnumerable<IMaptimeDataSource> maptimeDataSources;

            if (UseMaptimeDataSources(childMappingData, out maptimeDataSources))
            {
                foreach (var maptimeDataSource in maptimeDataSources)
                {
                    var finalDataSource = maptimeDataSource.WrapInFinalDataSource
                        ? GetFinalDataSource(maptimeDataSource, dataSourceIndex, childMappingData)
                        : maptimeDataSource;

                    yield return finalDataSource;

                    if (!finalDataSource.IsConditional)
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
            out IEnumerable<IConfiguredDataSource> configuredDataSources)
        {
            configuredDataSources = mapperData
                .MapperContext
                .UserConfigurations
                .GetDataSources(mapperData);

            return configuredDataSources.Any();
        }

        private bool UseMaptimeDataSources(
            IChildMemberMappingData childMappingData,
            out IEnumerable<IMaptimeDataSource> maptimeDataSources)
        {
            var applicableFactory = _mapTimeDataSourceFactories
                .FirstOrDefault(factory => factory.IsFor(childMappingData.MapperData));

            if (applicableFactory == null)
            {
                maptimeDataSources = Enumerable<IMaptimeDataSource>.Empty;
                return false;
            }

            maptimeDataSources = applicableFactory.Create(childMappingData);
            return true;
        }

        private static IEnumerable<IDataSource> GetSourceMemberDataSources(
            IEnumerable<IConfiguredDataSource> configuredDataSources,
            int dataSourceIndex,
            IChildMemberMappingData mappingData)
        {
            var bestMatchingSourceMember = SourceMemberMatcher.GetMatchFor(mappingData);
            var matchingSourceMemberDataSource = GetSourceMemberDataSourceOrNull(bestMatchingSourceMember, mappingData);

            if ((matchingSourceMemberDataSource == null) ||
                configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (dataSourceIndex == 0)
                {
                    if (mappingData.MapperData.TargetMember.IsComplex)
                    {
                        yield return new ComplexTypeMappingDataSource(dataSourceIndex, mappingData);
                    }
                }
                else
                {
                    yield return GetFallbackDataSourceFor(mappingData);
                }

                yield break;
            }

            yield return matchingSourceMemberDataSource;

            if (matchingSourceMemberDataSource.IsConditional)
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
            var childTargeMember = childMappingData.MapperData.TargetMember;

            if (UseComplexTypeDataSource(childTargeMember))
            {
                return new ComplexTypeMappingDataSource(foundDataSource, dataSourceIndex, childMappingData);
            }

            if (childTargeMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(foundDataSource, dataSourceIndex, childMappingData);
            }

            return foundDataSource;
        }

        private static bool UseComplexTypeDataSource(QualifiedMember targetMember)
        {
            if (!targetMember.IsComplex)
            {
                return false;
            }

            if (targetMember.Type == typeof(object))
            {
                return true;
            }

            return !targetMember.Type.IsFromBcl();
        }
    }
}