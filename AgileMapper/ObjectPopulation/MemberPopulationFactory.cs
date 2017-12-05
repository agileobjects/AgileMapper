namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using DataSources;
    using Extensions;
    using Members;
    using Members.Population;

    internal class MemberPopulationFactory
    {
        public static readonly MemberPopulationFactory Default = new MemberPopulationFactory(mapperData =>
            GlobalContext.Instance
                .MemberCache
                .GetTargetMembers(mapperData.TargetType)
                .Select(tm => mapperData.TargetMember.Append(tm)));

        private readonly Func<ObjectMapperData, IEnumerable<QualifiedMember>> _targetMembersFactory;

        public MemberPopulationFactory(Func<ObjectMapperData, IEnumerable<QualifiedMember>> targetMembersFactory)
        {
            _targetMembersFactory = targetMembersFactory;
        }

        public IEnumerable<IMemberPopulation> Create(IObjectMappingData mappingData)
        {
            return _targetMembersFactory
                .Invoke(mappingData.MapperData)
                .Select(tm =>
                {
                    var memberPopulation = Create(tm, mappingData);

                    if (memberPopulation.IsSuccessful ||
                        mappingData.MappingContext.AddUnsuccessfulMemberPopulations)
                    {
                        return memberPopulation;
                    }

                    return null;
                })
                .WhereNotNull();
        }

        private static IMemberPopulation Create(QualifiedMember targetMember, IObjectMappingData mappingData)
        {
            var childMapperData = new ChildMemberMapperData(targetMember, mappingData.MapperData);

            if (targetMember.IsUnmappable(out var reason))
            {
                return MemberPopulation.Unmappable(childMapperData, reason);
            }

            if (TargetMemberIsUnconditionallyIgnored(
                    childMapperData,
                    out var configuredIgnore,
                    out var populateCondition))
            {
                return MemberPopulation.IgnoredMember(childMapperData, configuredIgnore);
            }

            var childMappingData = mappingData.GetChildMappingData(childMapperData);
            var dataSources = DataSourceFinder.FindDataSources(childMappingData);

            if (dataSources.None)
            {
                return MemberPopulation.NoDataSource(childMapperData);
            }

            return MemberPopulation.WithRegistration(childMappingData, dataSources, populateCondition);
        }

        private static bool TargetMemberIsUnconditionallyIgnored(
            IMemberMapperData mapperData,
            out ConfiguredIgnoredMember configuredIgnore,
            out Expression populateCondition)
        {
            configuredIgnore = mapperData
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