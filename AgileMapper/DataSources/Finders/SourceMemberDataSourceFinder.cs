namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using Extensions.Internal;
    using Members;
    using System.Collections.Generic;

    internal struct SourceMemberDataSourceFinder : IDataSourceFinder
    {
        public IEnumerable<IDataSource> FindFor(DataSourceFindContext context)
        {
            if (context.MapperData.TargetMember.IsCustom)
            {
                yield break;
            }

            var matchingSourceMemberDataSource = GetSourceMemberDataSourceOrNull(context);
            var configuredDataSources = context.ConfiguredDataSources;
            var targetMember = context.MapperData.TargetMember;

            if ((matchingSourceMemberDataSource == null) ||
                 configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (context.DataSourceIndex == 0)
                {
                    if (UseFallbackComplexTypeMappingDataSource(targetMember))
                    {
                        yield return new ComplexTypeMappingDataSource(context.DataSourceIndex, context.ChildMappingData);
                    }
                }
                else if (configuredDataSources.Any() && configuredDataSources.Last().IsConditional)
                {
                    yield return context.GetFallbackDataSource();
                }

                yield break;
            }

            if (matchingSourceMemberDataSource.SourceMember.IsSimple &&
                context.MapperData.MapperContext.UserConfigurations.HasConfiguredToTargetDataSources)
            {
                var updatedMapperData = new ChildMemberMapperData(
                    matchingSourceMemberDataSource.SourceMember,
                    targetMember,
                    context.MapperData.Parent);

                var configuredRootDataSources = context
                    .MapperData
                    .MapperContext
                    .UserConfigurations
                    .GetDataSourcesForToTarget(updatedMapperData);

                foreach (var configuredRootDataSource in configuredRootDataSources)
                {
                    yield return configuredRootDataSource;
                }
            }

            yield return matchingSourceMemberDataSource;

            if (!targetMember.IsReadOnly &&
                 matchingSourceMemberDataSource.IsConditional &&
                (matchingSourceMemberDataSource.IsValid || configuredDataSources.Any()))
            {
                yield return context.GetFallbackDataSource();
            }
        }

        private static IDataSource GetSourceMemberDataSourceOrNull(DataSourceFindContext context)
        {
            var bestMatchingSourceMember = SourceMemberMatcher.GetMatchFor(
                context.ChildMappingData,
                out var contextMappingData);

            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            var sourceMemberDataSource = SourceMemberDataSource
                .For(bestMatchingSourceMember, contextMappingData.MapperData);

            return context.GetFinalDataSource(sourceMemberDataSource, contextMappingData);
        }

        private static bool UseFallbackComplexTypeMappingDataSource(QualifiedMember targetMember)
            => targetMember.IsComplex && !targetMember.IsDictionary && (targetMember.Type != typeof(object));
    }
}