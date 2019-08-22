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
    using DataSources.Factories;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;

    internal class MemberPopulationContext
    {
        private IList<ConfiguredIgnoredMember> _memberIgnores;
        private ConfiguredIgnoredMember _memberIgnore;

        public MemberPopulationContext(IObjectMappingData mappingData)
        {
            MappingData = mappingData;
        }

        public MappingRuleSet RuleSet => MappingContext.RuleSet;

        public MapperContext MapperContext => MappingContext.MapperContext;

        private UserConfigurationSet UserConfigurations => MapperContext.UserConfigurations;

        public IMappingContext MappingContext => MappingData.MappingContext;

        public IObjectMappingData MappingData { get; }

        private ObjectMapperData MapperData => MappingData.MapperData;

        public IMemberMapperData MemberMapperData { get; private set; }

        public QualifiedMember TargetMember => MemberMapperData.TargetMember;

        public bool AddUnsuccessfulMemberPopulations => MappingContext.AddUnsuccessfulMemberPopulations;

        public MemberPopulationContext With(QualifiedMember targetMember)
        {
            MemberMapperData = new ChildMemberMapperData(targetMember, MapperData);
            _memberIgnore = null;
            return this;
        }

        private IList<ConfiguredIgnoredMember> MemberIgnores
            => _memberIgnores ?? (_memberIgnores = UserConfigurations.GetMemberIgnoresFor(MemberMapperData));

        public ConfiguredIgnoredMember MemberIgnore
            => _memberIgnore ?? (_memberIgnore = MemberIgnores.FindMatch(MemberMapperData));

        public bool TargetMemberIsUnconditionallyIgnored(out Expression populateCondition)
        {
            if (MemberIgnore == null)
            {
                populateCondition = null;
                return false;
            }

            populateCondition = _memberIgnore.GetConditionOrNull(MemberMapperData);
            return (populateCondition == null);
        }
    }

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
            var populationContext = new MemberPopulationContext(mappingData);

            return _targetMembersFactory
                .Invoke(mappingData.MapperData)
                .Project(tm => Create(populationContext.With(tm)))
                .WhereNotNull();
        }

        private static IMemberPopulator Create(MemberPopulationContext context)
        {
            if (TargetMemberIsUnmappable(context, out var reason))
            {
                return MemberPopulator.Unmappable(context, reason);
            }

            if (context.TargetMemberIsUnconditionallyIgnored(out var populateCondition))
            {
                return MemberPopulator.IgnoredMember(context);
            }

            var childMappingData = context.MappingData.GetChildMappingData(context.MemberMapperData);
            var dataSources = DataSourceSetFactory.CreateFor(childMappingData);

            if (dataSources.None)
            {
                return MemberPopulator.NoDataSource(context);
            }

            return MemberPopulator.WithRegistration(dataSources, populateCondition);
        }

        private static bool TargetMemberIsUnmappable(MemberPopulationContext context, out string reason)
        {
            if (!context.RuleSet.Settings.AllowSetMethods &&
                (context.TargetMember.LeafMember.MemberType == MemberType.SetMethod))
            {
                reason = "Set methods are unsupported by rule set '" + context.RuleSet.Name + "'";
                return true;
            }

            if (TargetMemberWillBePopulatedByCtor(context))
            {
                reason = "Expected to be populated by constructor parameter";
                return true;
            }

            return context.MemberMapperData.TargetMemberIsUnmappable(
                context.TargetMember,
                md => md.MapperContext.UserConfigurations.QueryDataSourceFactories(md),
                context.MapperContext.UserConfigurations,
                out reason);
        }

        private static bool TargetMemberWillBePopulatedByCtor(MemberPopulationContext context)
        {
            if (!context.TargetMember.LeafMember.HasMatchingCtorParameter ||
                (context.RuleSet.Settings.RootHasPopulatedTarget && context.MappingData.IsRoot))
            {
                return false;
            }

            var creationInfos = context.MappingData.GetTargetObjectCreationInfos();

            return creationInfos.Any() &&
                   creationInfos.All(ci => ci.IsUnconditional && ci.HasCtorParameterFor(context.TargetMember.LeafMember));
        }
    }
}