namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Members;
    using Members.Population;

    internal class MemberPopulationFactory
    {
        public static MemberPopulationFactory Default = new MemberPopulationFactory(mapperData =>
            GlobalContext.Instance
                .MemberFinder
                .GetTargetMembers(mapperData.TargetType)
                .Select(tm => mapperData.TargetMember.Append(tm)));

        private readonly Func<IMemberMapperData, IEnumerable<QualifiedMember>> _targetMembersFactory;

        public MemberPopulationFactory(Func<IMemberMapperData, IEnumerable<QualifiedMember>> targetMembersFactory)
        {
            _targetMembersFactory = targetMembersFactory;
        }

        public IEnumerable<IMemberPopulation> Create(IObjectMappingData mappingData)
        {
            var memberPopulations = _targetMembersFactory
                .Invoke(mappingData.MapperData)
                .Select(tm => Create(tm, mappingData));

            return memberPopulations;
        }

        private IMemberPopulation Create(QualifiedMember targetMember, IObjectMappingData mappingData)
        {
            var childMapperData = new ChildMemberMapperData(targetMember, mappingData.MapperData);

            Expression populateCondition;

            if (TargetMemberIsUnconditionallyIgnored(childMapperData, out populateCondition))
            {
                return MemberPopulation.IgnoredMember(childMapperData);
            }

            var childMappingData = mappingData.GetChildMappingData(childMapperData);
            var dataSources = DataSourceFinder.FindDataSources(childMappingData);

            if (dataSources.None)
            {
                return MemberPopulation.NoDataSource(childMapperData);
            }

            return new MemberPopulation(childMappingData, dataSources, populateCondition);
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