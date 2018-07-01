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

            var matchingSourceMemberDataSource = GetSourceMemberDataSourceOrNull(context);
            var configuredDataSources = context.ConfiguredDataSources;
            var targetMember = context.MapperData.TargetMember;

            if ((matchingSourceMemberDataSource == null) ||
                 configuredDataSources.Any(cds => cds.IsSameAs(matchingSourceMemberDataSource)))
            {
                if (context.DataSourceIndex == 0)
                {
                    if (targetMember.IsComplex && (targetMember.Type != typeof(object)))
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
    }
}