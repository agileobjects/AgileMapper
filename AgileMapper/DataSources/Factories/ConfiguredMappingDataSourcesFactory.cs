namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using Members;
    using ObjectPopulation;

    internal static class ConfiguredMappingDataSourcesFactory
    {
        public static IEnumerable<IDataSource> Create(DataSourceFindContext context)
        {
            if (context.UseSourceMemberDataSource())
            {
                yield break;
            }

            var matchingSourceMember = context.BestSourceMemberMatch.SourceMember;

            if (matchingSourceMember == null)
            {
                yield break;
            }

            var childObjectMappingData = ObjectMappingDataFactory.ForChild(
                matchingSourceMember,
                context.TargetMember,
                context.DataSourceIndex,
                context.MemberMappingData.Parent);

            var mapping = ConfiguredMappingFactory
                .GetMappingOrNull(childObjectMappingData, out _);

            if (mapping == null)
            {
                yield break;
            }

            var childMapperData = context.MemberMapperData;
            var childObjectMapperData = childObjectMappingData.MapperData;
            var sourceMemberAccess = matchingSourceMember.GetRelativeQualifiedAccess(childMapperData);
            var targetMemberAccess = childMapperData.GetTargetMemberAccess();

            var mappingValues = new MappingValues(
                sourceMemberAccess,
                targetMemberAccess,
                childMapperData.ElementIndex,
                childMapperData.ElementKey);

            var directAccessMapping = MappingFactory.GetDirectAccessMapping(
                mapping,
                childObjectMapperData,
                mappingValues,
                MappingDataCreationFactory.ForChild(mappingValues, context.DataSourceIndex, childObjectMapperData));

            var returnLabel = childObjectMapperData
                .GetFinalisedReturnLabel(directAccessMapping, out _);

            yield return new AdHocDataSource(matchingSourceMember, returnLabel);
        }
    }
}