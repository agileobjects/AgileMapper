namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;

    internal static class SourceMemberDataSourceFactory
    {
        public static IEnumerable<IDataSource> Create(DataSourceFindContext context)
        {
            if (context.MapperData.TargetMember.IsCustom)
            {
                yield break;
            }

            var matchingSourceMemberDataSource = GetSourceMemberDataSource(context, out var hasUseableSourceMember);
            var configuredDataSources = context.ConfiguredDataSources;
            var targetMember = context.MapperData.TargetMember;

            if (!hasUseableSourceMember ||
                 configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (context.DataSourceIndex == 0)
                {
                    if (UseFallbackComplexTypeMappingDataSource(targetMember))
                    {
                        yield return new ComplexTypeDataSource(context.DataSourceIndex, context.ChildMappingData);
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

        private static IDataSource GetSourceMemberDataSource(
            DataSourceFindContext context,
            out bool hasUseableSourceMember)
        {
            var bestSourceMemberMatch = SourceMemberMatcher.GetMatchFor(context.ChildMappingData);
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

        private static bool UseFallbackComplexTypeMappingDataSource(QualifiedMember targetMember)
            => targetMember.IsComplex && !targetMember.IsDictionary && (targetMember.Type != typeof(object));
    }
}