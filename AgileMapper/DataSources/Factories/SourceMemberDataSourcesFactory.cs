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

            if (!context.UseSourceMemberDataSource())
            {
                if (context.DataSourceIndex == 0)
                {
                    if (context.UseFallbackComplexTypeDataSource())
                    {
                        yield return ComplexTypeDataSource.Create(context.DataSourceIndex, context.MemberMappingData);
                    }
                }
                else if (context.UseFallbackForConditionalConfiguredDataSource())
                {
                    yield return context.GetFallbackDataSource();
                }

                if (context.UseConfiguredDataSourcesOnly())
                {
                    yield break;
                }
            }

            if (context.ReturnSimpleTypeToTargetDataSources())
            {
                var updatedMapperData = new ChildMemberMapperData(
                    context.MatchingSourceMemberDataSource.SourceMember,
                    context.TargetMember,
                    context.MemberMapperData.Parent);

                var configuredToTargetDataSources = context
                    .MapperContext
                    .UserConfigurations
                    .GetDataSourcesForToTarget(updatedMapperData);

                foreach (var configuredDataSource in configuredToTargetDataSources)
                {
                    yield return configuredDataSource;
                }
            }

            yield return context.MatchingSourceMemberDataSource;

            if (context.UseFallbackDataSource())
            {
                yield return context.GetFallbackDataSource();
            }
        }

        private static bool UseFallbackComplexTypeDataSource(this DataSourceFindContext context)
        {
            var targetMember = context.TargetMember;

            return targetMember.IsComplex && !targetMember.IsDictionary && (targetMember.Type != typeof(object));
        }

        private static bool UseFallbackForConditionalConfiguredDataSource(this DataSourceFindContext context)
        {
            return context.ConfiguredDataSources.Any() &&
                   context.ConfiguredDataSources.Last().IsConditional &&
                  (context.MatchingSourceMemberDataSource.SourceMember != null);
        }

        private static bool UseConfiguredDataSourcesOnly(this DataSourceFindContext context)
        {
            return context.BestSourceMemberMatch.IsUseable ||
                  (context.MatchingSourceMemberDataSource.SourceMember == null);
        }

        private static bool ReturnSimpleTypeToTargetDataSources(this DataSourceFindContext context)
        {
            return context.MatchingSourceMemberDataSource.SourceMember.IsSimple &&
                   context.MapperContext.UserConfigurations.HasToTargetDataSources;
        }

        private static bool UseFallbackDataSource(this DataSourceFindContext context)
        {
            return !context.TargetMember.IsReadOnly &&
                    context.MatchingSourceMemberDataSource.IsConditional &&
                   (context.MatchingSourceMemberDataSource.IsValid || context.ConfiguredDataSources.Any());
        }
    }
}