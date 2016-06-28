namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;

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

        public DataSourceSet FindFor(IMemberMappingContext context)
            => new DataSourceSet(EnumerateSuccessfulDataSources(context).ToArray());

        public IDataSource DataSourceAt(int index, IMemberMappingContext context)
            => EnumerateSuccessfulDataSources(context).ElementAt(index);

        private IEnumerable<IDataSource> EnumerateSuccessfulDataSources(IMemberMappingContext context)
            => EnumerateDataSources(context).Where(dataSource => dataSource.IsValid);

        private IEnumerable<IDataSource> EnumerateDataSources(IMemberMappingContext context)
        {
            if (context.TargetMember.IsSimple)
            {
                yield return context.MappingContext.RuleSet.InitialDataSourceFactory.Create(context);
            }

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
                yield return new ComplexTypeMappingDataSource(context.SourceMember, dataSourceIndex, context);
                yield break;
            }

            foreach (var dataSource in GetSourceMemberDataSources(context, configuredDataSources, dataSourceIndex))
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

        private IEnumerable<IDataSource> GetSourceMemberDataSources(
            IMemberMappingContext context,
            IEnumerable<IConfiguredDataSource> configuredDataSources,
            int dataSourceIndex)
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

        public IDataSource GetSourceMemberDataSourceOrNull(IMemberMappingContext context)
        {
            if (context.Parent == null)
            {
                return GetSourceMemberDataSourceFor(
                    QualifiedMember.From(Member.RootSource(context.SourceType), context.MapperContext.NamingSettings),
                    0,
                    context);
            }

            return GetSourceMemberDataSourceOrNull(0, context);
        }

        private IDataSource GetSourceMemberDataSourceOrNull(int dataSourceIndex, IMemberMappingContext context)
        {
            var bestMatchingSourceMember = GetSourceMemberFor(context);

            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            bestMatchingSourceMember = bestMatchingSourceMember.RelativeTo(context.SourceMember);

            return GetSourceMemberDataSourceFor(bestMatchingSourceMember, dataSourceIndex, context);
        }

        private static IDataSource GetSourceMemberDataSourceFor(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMappingContext context)
        {
            return GetFinalDataSource(
                new SourceMemberDataSource(sourceMember, context),
                dataSourceIndex,
                context);
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

        public IQualifiedMember GetSourceMemberFor(IMemberMappingContext context)
        {
            var rootSourceMember = context.SourceMember;

            return GetAllSourceMembers(rootSourceMember, context)
                .FirstOrDefault(sm => IsMatchingMember(sm, context));
        }

        private static bool IsMatchingMember(IQualifiedMember sourceMember, IMemberMappingContext context)
        {
            return sourceMember.Matches(context.TargetMember) &&
                   context.MapperContext.ValueConverters.CanConvert(sourceMember.Type, context.TargetMember.Type);
        }

        private static IEnumerable<IQualifiedMember> GetAllSourceMembers(
            IQualifiedMember parentMember,
            IMemberMappingContext context)
        {
            yield return parentMember;

            if (!parentMember.CouldMatch(context.TargetMember))
            {
                yield break;
            }

            var parentMemberType = context.Parent.GetSourceMemberRuntimeType(parentMember);

            if (parentMemberType != parentMember.Type)
            {
                parentMember = parentMember.WithType(parentMemberType);
                yield return parentMember;
            }

            foreach (var sourceMember in context.Parent.GlobalContext.MemberFinder.GetReadableMembers(parentMember.Type))
            {
                var childMember = parentMember.Append(sourceMember);

                if (sourceMember.IsSimple)
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in GetAllSourceMembers(childMember, context))
                {
                    yield return qualifiedMember;
                }
            }
        }
    }
}