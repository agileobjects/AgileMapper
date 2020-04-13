namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;

    internal static class ConfiguredMappingDataSourcesFactory
    {
        public static IEnumerable<IDataSource> Create(DataSourceFindContext context)
        {
            if (context.UseSourceMemberDataSource())
            {
                yield break;
            }

            var matchingSourceMember = context.BestSourceMemberMatch.SourceMember;

            if (matchingSourceMember != null)
            {
                yield return ConfiguredMappingDataSource.Create(
                    matchingSourceMember,
                    context.DataSourceIndex,
                    context.MemberMappingData);
            }
        }
    }
}