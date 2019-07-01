namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;

    internal struct SourceMemberDataSourceFinder : IDataSourceFinder
    {
        public IEnumerable<IDataSource> FindFor(DataSourceFindContext context)
        {
            if (context.MapperData.TargetMember.IsCustom)
            {
                yield break;
            }

            var matchingSourceMemberDataSource = GetSourceMemberDataSource(context);
            var configuredDataSources = context.ConfiguredDataSources;
            var targetMember = context.MapperData.TargetMember;

            if (!matchingSourceMemberDataSource.IsValid ||
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

                if (matchingSourceMemberDataSource.SourceMember == null)
                {
                    yield break;
                }
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

            if (!matchingSourceMemberDataSource.IsValid)
            {
                yield break;
            }

            yield return matchingSourceMemberDataSource;

            if (!targetMember.IsReadOnly &&
                 matchingSourceMemberDataSource.IsConditional &&
                (matchingSourceMemberDataSource.IsValid || configuredDataSources.Any()))
            {
                yield return context.GetFallbackDataSource();
            }
        }

        private static IDataSource GetSourceMemberDataSource(DataSourceFindContext context)
        {
            var bestSourceMemberMatch = SourceMemberMatcher.GetMatchFor(context.ChildMappingData);

            if (bestSourceMemberMatch.IsUseable)
            {
                return context.GetFinalDataSource(
                    SourceMemberDataSource.For(bestSourceMemberMatch),
                    bestSourceMemberMatch.ContextMappingData);
            }

            return new AdHocDataSource(
                bestSourceMemberMatch.SourceMember,
                Constants.EmptyExpression);
        }

        private static bool UseFallbackComplexTypeMappingDataSource(QualifiedMember targetMember)
            => targetMember.IsComplex && !targetMember.IsDictionary && (targetMember.Type != typeof(object));
    }
}