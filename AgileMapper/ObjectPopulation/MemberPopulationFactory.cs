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
        public static MemberPopulationFactory Default = new MemberPopulationFactory(DataSourceFinder.FindDataSources);

        private readonly Func<IChildMemberMappingData, DataSourceSet> _dataSourcesFactory;

        public MemberPopulationFactory(Func<IChildMemberMappingData, DataSourceSet> dataSourcesFactory)
        {
            _dataSourcesFactory = dataSourcesFactory;
        }

        public IEnumerable<IMemberPopulation> Create(IObjectMappingData mappingData)
        {
            var memberPopulations = GlobalContext
                .Instance
                .MemberFinder
                .GetTargetMembers(mappingData.MapperData.TargetType)
                .Select(tm => Create(mappingData.MapperData.TargetMember.Append(tm), mappingData));

            return memberPopulations;
        }

        public IMemberPopulation Create(QualifiedMember targetMember, IObjectMappingData mappingData)
        {
            var childMapperData = new ChildMemberMapperData(targetMember, mappingData.MapperData);

            Expression populateCondition;

            if (TargetMemberIsUnconditionallyIgnored(childMapperData, out populateCondition))
            {
                return MemberPopulation.IgnoredMember(childMapperData);
            }

            var childMappingData = mappingData.GetChildMappingData(childMapperData);
            var dataSources = _dataSourcesFactory.Invoke(childMappingData);

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