namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal class DataSourceFinder
    {
        private readonly ICollection<IConditionalDataSourceFactory> _mapTimeDataSourceFactories;

        public DataSourceFinder()
        {
            _mapTimeDataSourceFactories = new List<IConditionalDataSourceFactory>
            {
                new DictionaryDataSourceFactory()
            };
        }

        public DataSourceSet FindFor(IMemberMappingData mappingData)
        {
            var validDataSources = EnumerateDataSources(mappingData)
                .Where(ds => ds.IsValid)
                .ToArray();

            if (mappingData.MapperData.TargetMember.IsSimple && validDataSources.Any())
            {
                var initialDataSource = mappingData
                    .RuleSet
                    .InitialDataSourceFactory
                    .Create(mappingData.MapperData);

                if (initialDataSource.IsValid)
                {
                    validDataSources = validDataSources.Prepend(initialDataSource).ToArray();
                }
            }

            return new DataSourceSet(validDataSources);
        }

        private IEnumerable<IDataSource> EnumerateDataSources(IMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var maptimeDataSource = GetMaptimeDataSourceOrNull(mapperData);

            if (maptimeDataSource != null)
            {
                yield return maptimeDataSource;
                yield break;
            }

            var dataSourceIndex = 0;

            IEnumerable<IConfiguredDataSource> configuredDataSources;

            if (DataSourcesAreConfigured(mapperData, out configuredDataSources))
            {
                foreach (var configuredDataSource in configuredDataSources)
                {
                    yield return GetFinalDataSource(configuredDataSource, dataSourceIndex, mappingData);

                    if (!configuredDataSource.IsConditional)
                    {
                        yield break;
                    }

                    ++dataSourceIndex;
                }
            }

            var sourceMemberDataSources =
                GetSourceMemberDataSources(configuredDataSources, dataSourceIndex, mappingData);

            foreach (var dataSource in sourceMemberDataSources)
            {
                yield return dataSource;
            }
        }

        private IDataSource GetMaptimeDataSourceOrNull(IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsComplex)
            {
                return null;
            }

            return _mapTimeDataSourceFactories
                .FirstOrDefault(factory => factory.IsFor(mapperData))?
                .Create(mapperData);
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

        private static IDataSource FallbackDataSourceFor(IMemberMapperData mapperData)
            => mapperData.RuleSet.FallbackDataSourceFactory.Create(mapperData);

        private static IEnumerable<IDataSource> GetSourceMemberDataSources(
            IEnumerable<IConfiguredDataSource> configuredDataSources,
            int dataSourceIndex,
            IMemberMappingData mappingData)
        {
            var bestMatchingSourceMember = SourceMemberMatcher.GetMatchFor(mappingData);
            var mapperData = mappingData.MapperData;
            var matchingSourceMemberDataSource = GetSourceMemberDataSourceOrNull(bestMatchingSourceMember, mappingData);

            if ((matchingSourceMemberDataSource == null) ||
                configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (dataSourceIndex == 0)
                {
                    if (mapperData.TargetMember.IsComplex)
                    {
                        yield return new ComplexTypeMappingDataSource(dataSourceIndex, mappingData);
                    }
                }
                else
                {
                    yield return FallbackDataSourceFor(mapperData);
                }

                yield break;
            }

            yield return matchingSourceMemberDataSource;

            if (matchingSourceMemberDataSource.IsConditional)
            {
                yield return FallbackDataSourceFor(mapperData);
            }
        }

        private static IDataSource GetSourceMemberDataSourceOrNull(
            IQualifiedMember bestMatchingSourceMember,
            IMemberMappingData mappingData)
        {
            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            bestMatchingSourceMember = bestMatchingSourceMember.RelativeTo(mappingData.MapperData.SourceMember);
            var sourceMemberDataSource = new SourceMemberDataSource(bestMatchingSourceMember, mappingData.MapperData);

            return GetFinalDataSource(sourceMemberDataSource, 0, mappingData);
        }

        private static IDataSource GetFinalDataSource(
            IDataSource foundDataSource,
            int dataSourceIndex,
            IMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (UseComplexTypeDataSource(mapperData.TargetMember))
            {
                return new ComplexTypeMappingDataSource(foundDataSource, dataSourceIndex, mappingData);
            }

            if (mapperData.TargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(foundDataSource, dataSourceIndex, mappingData);
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

            return targetMember.Type.GetAssembly() != typeof(string).GetAssembly();
        }
    }
}