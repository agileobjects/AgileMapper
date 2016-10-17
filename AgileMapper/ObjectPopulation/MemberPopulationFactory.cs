namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal static class MemberPopulationFactory
    {
        public static IEnumerable<IMemberPopulation> Create(IObjectMappingData mappingData)
        {
            return GlobalContext
                .Instance
                .MemberFinder
                .GetWriteableMembers(mappingData.TargetType)
                .Select(targetMember => Create(targetMember, mappingData));
        }

        private static IMemberPopulation Create(Member targetMember, IObjectMappingData mappingData)
        {
            var qualifiedMember = mappingData.MapperData.TargetMember.Append(targetMember);
            var childMapperData = new MemberMapperData(qualifiedMember, mappingData.MapperData);

            Expression populateCondition;

            if (TargetMemberIsUnconditionallyIgnored(childMapperData, out populateCondition))
            {
                return MemberPopulation.IgnoredMember(childMapperData);
            }

            var childMappingData = mappingData.GetChildMappingData(childMapperData);

            var dataSources = childMapperData
                .MapperContext
                .DataSources
                .FindFor(childMappingData);

            if (dataSources.None)
            {
                return MemberPopulation.NoDataSource(childMapperData);
            }

            return new MemberPopulation(childMapperData, dataSources, populateCondition);
        }

        private static bool TargetMemberIsUnconditionallyIgnored(
            IMemberMapperData mapperData,
            out Expression populateCondition)
        {
            var configuredIgnore = mapperData
                .MapperContext
                .UserConfigurations
                .GetMemberIgnoreOrNull(mapperData);

            if (configuredIgnore == null)
            {
                populateCondition = null;
                return false;
            }

            populateCondition = configuredIgnore.GetConditionOrNull(mapperData);
            return (populateCondition == null);
        }
    }
}