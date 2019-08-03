  namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using DataSources.Finders;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;

    internal class MemberPopulatorFactory
    {
        public static readonly MemberPopulatorFactory Default = new MemberPopulatorFactory(mapperData =>
            GlobalContext.Instance
                .MemberCache
                .GetTargetMembers(mapperData.TargetType)
                .ProjectToArray(mapperData.TargetMember.Append));

        private readonly Func<ObjectMapperData, IEnumerable<QualifiedMember>> _targetMembersFactory;

        public MemberPopulatorFactory(Func<ObjectMapperData, IEnumerable<QualifiedMember>> targetMembersFactory)
        {
            _targetMembersFactory = targetMembersFactory;
        }

        public IEnumerable<IMemberPopulator> Create(IObjectMappingData mappingData)
        {
            return _targetMembersFactory
                .Invoke(mappingData.MapperData)
                .Project(tm =>
                {
                    var memberPopulator = Create(tm, mappingData);

                    if (memberPopulator.CanPopulate ||
                        mappingData.MappingContext.AddUnsuccessfulMemberPopulations)
                    {
                        return memberPopulator;
                    }
                    
                    return null;
                })
                .WhereNotNull();
        }

        private static IMemberPopulator Create(QualifiedMember targetMember, IObjectMappingData mappingData)
        {
            var childMapperData = new ChildMemberMapperData(targetMember, mappingData.MapperData);

            if (TargetMemberIsUnmappable(childMapperData, mappingData, out var reason))
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

            return MemberPopulator.WithRegistration(dataSources, populateCondition);
        }

        private static bool TargetMemberIsUnmappable(
            IMemberMapperData mapperData,
            IObjectMappingData mappingData,
            out string reason)
        {
            if (!mapperData.RuleSet.Settings.AllowSetMethods &&
                (mapperData.TargetMember.LeafMember.MemberType == MemberType.SetMethod))
            {
                reason = "Set methods are unsupported by rule set '" + mapperData.RuleSet.Name + "'";
                return true;
            }

            if (TargetMemberWillBePopulatedByCtor(mapperData, mappingData))
            {
                reason = "Expected to be populated by constructor parameter";
                return true;
            }

            return mapperData.TargetMemberIsUnmappable(
                mapperData.TargetMember,
                md => md.MapperContext.UserConfigurations.QueryDataSourceFactories(md),
                mapperData.MapperContext.UserConfigurations,
                out reason);
        }

        private static bool TargetMemberWillBePopulatedByCtor(IMemberMapperData mapperData, IObjectMappingData mappingData)
        {
            if (!mapperData.TargetMember.LeafMember.HasMatchingCtorParameter ||
                (mapperData.RuleSet.Settings.RootHasPopulatedTarget && (mapperData.Parent?.IsRoot == true)))
            {
                return false;
            }

            var creationInfos = mappingData.GetTargetObjectCreationInfos();

            return creationInfos.Any() &&
                   creationInfos.All(ci => ci.IsUnconditional && ci.HasCtorParameterFor(mapperData.TargetMember.LeafMember));
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