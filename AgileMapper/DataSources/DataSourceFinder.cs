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

        public DataSourceFinder(GlobalContext globalContext)
        {
            _cache = globalContext.CreateCache<DataSourceSetKey, DataSourceSet>();

            _mapTimeDataSourceFactories = new List<IConditionalDataSourceFactory>
            {
                new DictionaryDataSourceFactory()
            };
        }

        public DataSourceSet FindFor(IMemberMappingContext context)
        {
            var cacheKey = new DataSourceSetKey(context);

            return _cache.GetOrAdd(cacheKey, k =>
            {
                var validDataSources = EnumerateDataSources(context)
                    .Where(ds => ds.IsValid)
                    .ToArray();

                if (context.TargetMember.IsSimple && validDataSources.Any())
                {
                    var initialDataSource = context
                        .MappingContext
                        .RuleSet
                        .InitialDataSourceFactory
                        .Create(context);

                    if (initialDataSource.IsValid)
                    {
                        validDataSources = validDataSources.Prepend(initialDataSource).ToArray();
                    }
                }

                return new DataSourceSet(validDataSources);
            });
        }

        private IEnumerable<IDataSource> EnumerateDataSources(IMemberMappingContext context)
        {
            var maptimeDataSource = GetMaptimeDataSourceOrNull(context);

            if (maptimeDataSource != null)
            {
                yield return maptimeDataSource;
                yield break;
            }

            var dataSourceIndex = 0;

            IEnumerable<IConfiguredDataSource> configuredDataSources;

            if (DataSourcesAreConfigured(context, out configuredDataSources))
            {
                foreach (var configuredDataSource in configuredDataSources)
                {
                    yield return GetFinalDataSource(configuredDataSource, dataSourceIndex, context);

                    if (!configuredDataSource.IsConditional)
                    {
                        yield break;
                    }

                    ++dataSourceIndex;
                }
            }

            if (context.TargetMember.IsComplex)
            {
                yield return new ComplexTypeMappingDataSource(dataSourceIndex, context);
                yield break;
            }

            foreach (var dataSource in GetSourceMemberDataSources(configuredDataSources, dataSourceIndex, context))
            {
                yield return dataSource;
            }
        }

        private IDataSource GetMaptimeDataSourceOrNull(IMemberMappingContext context)
        {
            if (context.TargetMember.IsComplex)
            {
                return null;
            }

            return _mapTimeDataSourceFactories
                .FirstOrDefault(factory => factory.IsFor(context))?
                .Create(context);
        }

        private static bool DataSourcesAreConfigured(
            IMemberMappingContext context,
            out IEnumerable<IConfiguredDataSource> configuredDataSources)
        {
            configuredDataSources = context
                .MapperContext
                .UserConfigurations
                .GetDataSources(context);

            return configuredDataSources.Any();
        }

        private static IDataSource FallbackDataSourceFor(IMemberMappingContext context)
            => context.MappingContext.RuleSet.FallbackDataSourceFactory.Create(context);

        private static IEnumerable<IDataSource> GetSourceMemberDataSources(
            IEnumerable<IConfiguredDataSource> configuredDataSources,
            int dataSourceIndex,
            IMemberMappingContext context)
        {
            var matchingSourceMemberDataSource = GetSourceMemberDataSourceOrNull(context);

            if ((matchingSourceMemberDataSource == null) ||
                configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (dataSourceIndex > 0)
                {
                    yield return FallbackDataSourceFor(context);
                }

                yield break;
            }

            yield return matchingSourceMemberDataSource;

            if (matchingSourceMemberDataSource.IsConditional)
            {
                yield return FallbackDataSourceFor(context);
            }
        }

        private static IDataSource GetSourceMemberDataSourceOrNull(IMemberMappingContext context)
        {
            var bestMatchingSourceMember = SourceMemberMatcher.GetMatchFor(context);

            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            bestMatchingSourceMember = bestMatchingSourceMember.RelativeTo(context.SourceMember);
            var sourceMemberDataSource = new SourceMemberDataSource(bestMatchingSourceMember, context);

            return GetFinalDataSource(sourceMemberDataSource, 0, context);
        }

        private static IDataSource GetFinalDataSource(
            IDataSource foundDataSource,
            int dataSourceIndex,
            IMemberMappingContext context)
        {
            if (context.TargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(foundDataSource, dataSourceIndex, context);
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

            public DataSourceSetKey(IMemberMappingContext context)
            {
                _sourceType = context.SourceType;
                _ruleSet = context.MappingContext.RuleSet;
                _targetMemberSignature = context.TargetMember.Signature;
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