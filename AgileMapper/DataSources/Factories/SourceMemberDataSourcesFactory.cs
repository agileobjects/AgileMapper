namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;

    internal static class SourceMemberDataSourcesFactory
    {
        public static IEnumerable<IDataSource> Create(DataSourceFindContext context)
        {
            if (context.TargetMember.IsCustom)
            {
                yield break;
            }

            var matchingSourceMemberDataSource = GetSourceMemberDataSource(context, out var hasUseableSourceMember);
            var configuredDataSources = context.ConfiguredDataSources;
            var targetMember = context.TargetMember;

            if (!hasUseableSourceMember ||
                 configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (context.DataSourceIndex == 0)
                {
                    if (UseFallbackComplexTypeDataSource(targetMember))
                    {
                        yield return ComplexTypeDataSource.Create(context.DataSourceIndex, context.MemberMappingData);
                    }
                }
                else if (configuredDataSources.Any() && configuredDataSources.Last().IsConditional)
                {
                    yield return context.GetFallbackDataSource();
                }

                if (hasUseableSourceMember || 
                   (matchingSourceMemberDataSource.SourceMember == null))
                {
                    yield break;
                }
            }

            if (matchingSourceMemberDataSource.SourceMember.IsSimple &&
                context.MapperContext.UserConfigurations.HasConfiguredToTargetDataSources)
            {
                var updatedMapperData = new ChildMemberMapperData(
                    matchingSourceMemberDataSource.SourceMember,
                    targetMember,
                    context.MemberMapperData.Parent);

                var configuredRootDataSources = context
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

        private static IDataSource GetSourceMemberDataSource(
            DataSourceFindContext context,
            out bool hasUseableSourceMember)
        {
            var sourceMemberMatchContext = context.GetSourceMemberMatchContext();
            var bestSourceMemberMatch = SourceMemberMatcher.GetMatchFor(sourceMemberMatchContext);
            hasUseableSourceMember = bestSourceMemberMatch.IsUseable;

            if (hasUseableSourceMember)
            {
                return context.GetFinalDataSource(
                    bestSourceMemberMatch.CreateDataSource(),
                    bestSourceMemberMatch.ContextMappingData);
            }

            return new AdHocDataSource(
                bestSourceMemberMatch.SourceMember,
                Constants.EmptyExpression);
        }

        private static bool UseFallbackComplexTypeDataSource(QualifiedMember targetMember)
            => targetMember.IsComplex && !targetMember.IsDictionary && (targetMember.Type != typeof(object));
    }
}