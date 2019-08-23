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

            var matchingSourceMemberDataSource = GetSourceMemberDataSource(context);
            var configuredDataSources = context.ConfiguredDataSources;
            var targetMember = context.TargetMember;

            if (!context.BestSourceMemberMatch.IsUseable ||
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

                if (context.BestSourceMemberMatch.IsUseable ||
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

        private static IDataSource GetSourceMemberDataSource(DataSourceFindContext context)
        {
            var sourceMemberMatchContext = context.GetSourceMemberMatchContext();
            context.BestSourceMemberMatch = SourceMemberMatcher.GetMatchFor(sourceMemberMatchContext);

            if (context.BestSourceMemberMatch.IsUseable)
            {
                return context.GetFinalDataSource(
                    context.BestSourceMemberMatch.CreateDataSource(),
                    context.BestSourceMemberMatch.ContextMappingData);
            }

            return new AdHocDataSource(
                context.BestSourceMemberMatch.SourceMember,
                Constants.EmptyExpression);
        }

        private static bool UseFallbackComplexTypeDataSource(QualifiedMember targetMember)
            => targetMember.IsComplex && !targetMember.IsDictionary && (targetMember.Type != typeof(object));
    }
}