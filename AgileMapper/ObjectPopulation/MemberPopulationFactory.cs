namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal static class MemberPopulationFactory
    {
        public static IEnumerable<IMemberPopulation> Create(IObjectMapperCreationData data)
        {
            return GlobalContext
                .Instance
                .MemberFinder
                .GetWriteableMembers(data.MapperData.TargetType)
                .Select(targetMember => Create(targetMember, data));
        }

        private static IMemberPopulation Create(Member targetMember, IObjectMapperCreationData data)
        {
            var qualifiedMember = data.TargetMember.Append(targetMember);
            var childMapperData = new MemberMapperData(qualifiedMember, data.MapperData);

            Expression populateCondition;

            if (TargetMemberPopulationIsConditional(childMapperData, out populateCondition))
            {
                return MemberPopulation.IgnoredMember(childMapperData);
            }

            var childMapperCreationData = data.GetChildCreationData(childMapperData);

            var dataSources = childMapperData
                .MapperContext
                .DataSources
                .FindFor(childMapperCreationData);

            if (dataSources.None)
            {
                return MemberPopulation.NoDataSource(childMapperData);
            }

            return new MemberPopulation(childMapperData, dataSources, populateCondition);
        }

        private static bool TargetMemberPopulationIsConditional(
            MemberMapperData mapperData,
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