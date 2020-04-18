namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
    using System.Collections.Generic;
    using DataSources.Factories;
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
            var populationContext = new MemberPopulationContext(mappingData);

            return _targetMembersFactory
                .Invoke(mappingData.MapperData)
                .Project(populationContext, (ctx, tm) => Create(ctx.With(tm)))
                .WhereNotNull();
        }

        private static IMemberPopulator Create(MemberPopulationContext context)
        {
            if (TargetMemberIsUnmappable(context, out var reason))
            {
                return NullMemberPopulator.Unmappable(context, reason);
            }

            if (context.TargetMemberIsUnconditionallyIgnored(out var populateCondition))
            {
                return NullMemberPopulator.IgnoredMember(context);
            }

            var dataSourceFindContext = context.GetDataSourceFindContext();
            var dataSources = MemberDataSourceSetFactory.CreateFor(dataSourceFindContext);

            if (dataSources.None)
            {
                return NullMemberPopulator.NoDataSources(context);
            }

            dataSourceFindContext.MemberMapperData.RegisterTargetMemberDataSources(dataSources);

            return new MemberPopulator(
                dataSources,
                dataSourceFindContext.MemberMapperData,
                populateCondition);
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