namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Caching;
    using Extensions;
    using Members;

    internal class DataSourceFinder
    {
        private readonly ICache<DataSourceSetKey, DataSourceSet> _cache;
        private readonly ICollection<IConditionalDataSourceFactory> _mapTimeDataSourceFactories;

        public DataSourceFinder()
        {
            _cache = GlobalContext.Instance.CreateCache<DataSourceSetKey, DataSourceSet>();

            _mapTimeDataSourceFactories = new List<IConditionalDataSourceFactory>
            {
                new DictionaryDataSourceFactory()
            };
        }

        public DataSourceSet FindFor(IMemberMapperCreationData data)
        {
            var cacheKey = new DataSourceSetKey(data);

            return _cache.GetOrAdd(cacheKey, k =>
            {
                var validDataSources = EnumerateDataSources(data)
                    .Where(ds => ds.IsValid)
                    .ToArray();

                if (data.MapperData.TargetMember.IsSimple && validDataSources.Any())
                {
                    var initialDataSource = data
                        .RuleSet
                        .InitialDataSourceFactory
                        .Create(data.MapperData);

                    if (initialDataSource.IsValid)
                    {
                        validDataSources = validDataSources.Prepend(initialDataSource).ToArray();
                    }
                }

                return new DataSourceSet(validDataSources);
            });
        }

        private IEnumerable<IDataSource> EnumerateDataSources(IMemberMapperCreationData data)
        {
            var mmd = data.MapperData;

            var maptimeDataSource = GetMaptimeDataSourceOrNull(mmd);

            if (maptimeDataSource != null)
            {
                yield return maptimeDataSource;
                yield break;
            }

            var dataSourceIndex = 0;

            IEnumerable<IConfiguredDataSource> configuredDataSources;

            if (DataSourcesAreConfigured(mmd, out configuredDataSources))
            {
                foreach (var configuredDataSource in configuredDataSources)
                {
                    yield return GetFinalDataSource(configuredDataSource, dataSourceIndex, mmd);

                    if (!configuredDataSource.IsConditional)
                    {
                        yield break;
                    }

                    ++dataSourceIndex;
                }
            }

            var bestMatchingSourceMember = SourceMemberMatcher.GetMatchFor(data);

            if (mmd.TargetMember.IsComplex)
            {
                yield return new ComplexTypeMappingDataSource(bestMatchingSourceMember, dataSourceIndex, mmd);
                yield break;
            }

            var sourceMemberDataSources = GetSourceMemberDataSources(
                bestMatchingSourceMember,
                configuredDataSources,
                dataSourceIndex,
                data);

            foreach (var dataSource in sourceMemberDataSources)
            {
                yield return dataSource;
            }
        }

        private IDataSource GetMaptimeDataSourceOrNull(MemberMapperData data)
        {
            if (data.TargetMember.IsComplex)
            {
                return null;
            }

            return _mapTimeDataSourceFactories
                .FirstOrDefault(factory => factory.IsFor(data))?
                .Create(data);
        }

        private static bool DataSourcesAreConfigured(
            MemberMapperData data,
            out IEnumerable<IConfiguredDataSource> configuredDataSources)
        {
            configuredDataSources = data
                .MapperContext
                .UserConfigurations
                .GetDataSources(data);

            return configuredDataSources.Any();
        }

        private static IDataSource FallbackDataSourceFor(MemberMapperData data)
            => data.RuleSet.FallbackDataSourceFactory.Create(data);

        private static IEnumerable<IDataSource> GetSourceMemberDataSources(
            IQualifiedMember bestMatchingSourceMember,
            IEnumerable<IConfiguredDataSource> configuredDataSources,
            int dataSourceIndex,
            IMemberMapperCreationData data)
        {
            var matchingSourceMemberDataSource = GetSourceMemberDataSourceOrNull(bestMatchingSourceMember, data);

            if ((matchingSourceMemberDataSource == null) ||
                configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (dataSourceIndex > 0)
                {
                    yield return FallbackDataSourceFor(data.MapperData);
                }

                yield break;
            }

            yield return matchingSourceMemberDataSource;

            if (matchingSourceMemberDataSource.IsConditional)
            {
                yield return FallbackDataSourceFor(data.MapperData);
            }
        }

        private static IDataSource GetSourceMemberDataSourceOrNull(
            IQualifiedMember bestMatchingSourceMember,
            IMemberMapperCreationData data)
        {
            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            bestMatchingSourceMember = bestMatchingSourceMember.RelativeTo(data.MapperData.SourceMember);
            var sourceMemberDataSource = new SourceMemberDataSource(bestMatchingSourceMember, data.MapperData);

            return GetFinalDataSource(sourceMemberDataSource, 0, data.MapperData);
        }

        private static IDataSource GetFinalDataSource(
            IDataSource foundDataSource,
            int dataSourceIndex,
            MemberMapperData data)
        {
            if (data.TargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(foundDataSource, dataSourceIndex, data);
            }

            return foundDataSource;
        }

        public void Reset()
        {
            _cache.Empty();
        }

        private class DataSourceSetKey
        {
            private readonly Type _sourceType;
            private readonly MappingRuleSet _ruleSet;
            private readonly string _targetMemberSignature;

            public DataSourceSetKey(IMapperCreationData data)
            {
                _sourceType = data.SourceMember.Type;
                _ruleSet = data.RuleSet;
                _targetMemberSignature = data.TargetMember.Signature;
            }

            public override bool Equals(object obj)
            {
                var otherKey = (DataSourceSetKey)obj;

                return
                    (_sourceType == otherKey._sourceType) &&
                    (_ruleSet == otherKey._ruleSet) &&
                    (_targetMemberSignature == otherKey._targetMemberSignature);
            }

            public override int GetHashCode() => 0;
        }
    }
}