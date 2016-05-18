namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;
    using ObjectPopulation;

    internal class DataSourceFinder
    {
        public IEnumerable<IDataSource> FindFor(IMemberMappingContext context)
        {
            return EnumerateDataSources(context)
                .Where(dataSource => dataSource.IsSuccessful)
                .ToArray();
        }

        private IEnumerable<IDataSource> EnumerateDataSources(IMemberMappingContext context)
        {
            if (context.Parent == null)
            {
                yield return RootSourceMemberDataSourceFor(context);
                yield break;
            }

            var dataSourceIndex = 0;

            IEnumerable<IDataSource> configuredDataSources;

            if (DataSourcesAreConfigured(context, out configuredDataSources))
            {
                foreach (var configuredDataSource in configuredDataSources)
                {
                    yield return configuredDataSource;

                    if (!configuredDataSource.IsConditional)
                    {
                        yield break;
                    }

                    ++dataSourceIndex;
                }
            }

            if (context.TargetMember.IsComplex)
            {
                yield return new ComplexTypeMappingDataSource(context.SourceMember, context, dataSourceIndex);
                yield return FallbackDataSourceFor(context);
                yield break;
            }

            var matchingSourceMemberDataSource = GetSourceMemberDataSourceOrNull(context);

            if ((matchingSourceMemberDataSource != null) &&
                SourceMemberDataSourceIsUnconfigured(configuredDataSources, matchingSourceMemberDataSource))
            {
                yield return matchingSourceMemberDataSource;
            }

            yield return FallbackDataSourceFor(context);
        }

        private static IDataSource RootSourceMemberDataSourceFor(IMemberMappingContext context)
            => GetSourceMemberDataSourceFor(QualifiedMember.From(Member.RootSource(context.SourceObject.Type)), context);

        private static bool DataSourcesAreConfigured(IMemberMappingContext context, out IEnumerable<IDataSource> configuredDataSources)
        {
            var configuredDataSource = context
                .MapperContext
                .UserConfigurations
                .GetDataSourceOrNull(context);

            if (configuredDataSource == null)
            {
                configuredDataSources = Enumerable.Empty<IDataSource>();
                return false;
            }

            configuredDataSources = new[] { GetFinalDataSource(configuredDataSource, 0, context) };
            return true;
        }

        private static IDataSource FallbackDataSourceFor(IMemberMappingContext context)
            => context.MappingContext.RuleSet.FallbackDataSourceFactory.Create(context);

        public IDataSource GetSourceMemberDataSourceOrNull(IMemberMappingContext context)
        {
            var bestMatchingSourceMember = GetSourceMemberFor(context);

            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            bestMatchingSourceMember = bestMatchingSourceMember.RelativeTo(context.SourceObjectDepth);

            return GetSourceMemberDataSourceFor(bestMatchingSourceMember, context);
        }

        private static IDataSource GetSourceMemberDataSourceFor(IQualifiedMember sourceMember, IMemberMappingContext context)
            => GetFinalDataSource(new SourceMemberDataSource(sourceMember, context), 0, context);

        private static IDataSource GetFinalDataSource(
            IDataSource foundDataSource,
            int dataSourceIndex,
            IMemberMappingContext context)
        {
            if (context.TargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(foundDataSource, context, dataSourceIndex);
            }

            return foundDataSource;
        }

        public IQualifiedMember GetSourceMemberFor(IMemberMappingContext context)
        {
            var rootSourceMember = context.SourceMember;

            return GetAllSourceMembers(rootSourceMember, context.Parent)
                .FirstOrDefault(sm => IsMatchingMember(sm, context));
        }

        private static bool IsMatchingMember(IQualifiedMember sourceMember, IMemberMappingContext context)
        {
            return sourceMember.Matches(context.TargetMember) &&
                   context.MapperContext.ValueConverters.CanConvert(sourceMember.Type, context.TargetMember.Type);
        }

        private static IEnumerable<IQualifiedMember> GetAllSourceMembers(
            IQualifiedMember parentMember,
            IObjectMappingContext currentOmc)
        {
            yield return parentMember;

            var parentMemberType = currentOmc.GetSourceMemberRuntimeType(parentMember);

            foreach (var sourceMember in currentOmc.GlobalContext.MemberFinder.GetSourceMembers(parentMemberType))
            {
                var childMember = parentMember.Append(sourceMember);

                if (sourceMember.IsSimple)
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in GetAllSourceMembers(childMember, currentOmc))
                {
                    yield return qualifiedMember;
                }
            }
        }

        private static bool SourceMemberDataSourceIsUnconfigured(
            IEnumerable<IDataSource> configuredDataSources,
            IDataSource sourceMemberDataSource)
        {
            var sourceMemberDataSourceValue = sourceMemberDataSource.Value.ToString();

            return configuredDataSources
                .Select(cds => cds.Value.ToString())
                .All(cds => cds != sourceMemberDataSourceValue);
        }
    }
}