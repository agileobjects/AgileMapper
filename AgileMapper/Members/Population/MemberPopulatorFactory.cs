namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using DataSources;
    using DataSources.Finders;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;

    internal class MemberPopulatorFactory
    {
        public static readonly MemberPopulatorFactory Default = new MemberPopulatorFactory(mapperData =>
            GlobalContext.Instance
                .MemberCache
                .GetTargetMembers(mapperData.TargetType)
                .Select(tm => mapperData.TargetMember.Append(tm)));

        private readonly Func<ObjectMapperData, IEnumerable<QualifiedMember>> _targetMembersFactory;

        public MemberPopulatorFactory(Func<ObjectMapperData, IEnumerable<QualifiedMember>> targetMembersFactory)
        {
            _targetMembersFactory = targetMembersFactory;
        }

        public IEnumerable<IMemberPopulator> Create(IObjectMappingData mappingData)
        {
            return _targetMembersFactory
                .Invoke(mappingData.MapperData)
                .Select(tm =>
                {
                    var memberPopulation = Create(tm, mappingData);

                    if (memberPopulation.CanPopulate ||
                        mappingData.MappingContext.AddUnsuccessfulMemberPopulations)
                    {
                        return memberPopulation;
                    }

                    return null;
                })
                .WhereNotNull();
        }

        private static IMemberPopulator Create(QualifiedMember targetMember, IObjectMappingData mappingData)
        {
            var childMapperData = new ChildMemberMapperData(targetMember, mappingData.MapperData);

            if (childMapperData.TargetMemberIsUnmappable(out var reason))
            {
                return MemberPopulator.Unmappable(childMapperData, reason);
            }

            if (TargetMemberIsUnconditionallyIgnored(
                    childMapperData,
                    out var configuredIgnore,
                    out var populateCondition))
            {
                return MemberPopulator.IgnoredMember(childMapperData, configuredIgnore);
            }

            var childMappingData = mappingData.GetChildMappingData(childMapperData);
            var dataSources = DataSourceFinder.FindFor(childMappingData);

            if (dataSources.None)
            {
                return MemberPopulator.NoDataSource(childMapperData);
            }

            return MemberPopulator.WithRegistration(childMappingData, dataSources, populateCondition);
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