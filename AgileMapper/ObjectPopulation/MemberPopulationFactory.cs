namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    internal static class MemberPopulationFactory
    {
        public static IEnumerable<IMemberPopulation> Create(IObjectMappingContext omc)
        {
            return omc
                .GlobalContext
                .MemberFinder
                .GetTargetMembers(omc.ExistingObject.Type)
                .Select(targetMember => Create(targetMember, omc))
                .ToArray();
        }

        private static IMemberPopulation Create(Member targetMember, IObjectMappingContext omc)
        {
            var qualifiedMember = omc.TargetMember.Append(targetMember);
            var memberContext = new MemberMappingContext(qualifiedMember, omc);

            Expression ignoreCondition;

            if (TargetMemberIsIgnored(memberContext, omc, out ignoreCondition) &&
                (ignoreCondition == null))
            {
                return MemberPopulation.IgnoredMember(targetMember, omc);
            }

            var dataSource = omc
                .MapperContext
                .DataSources
                .FindFor(qualifiedMember, omc);

            if (dataSource == null)
            {
                return MemberPopulation.NoDataSource(targetMember, omc);
            }

            var population = new MemberPopulation(targetMember, dataSource, omc);

            AddConditions(dataSource, memberContext, population, ignoreCondition);

            return population;
        }

        private static bool TargetMemberIsIgnored(
            IMemberMappingContext context,
            IObjectMappingContext omc,
            out Expression ignoreCondition)
        {
            return omc.MapperContext.UserConfigurations.IsIgnored(context, out ignoreCondition);
        }

        private static void AddConditions(
            IDataSource dataSource,
            IMemberMappingContext context,
            IMemberPopulation population,
            Expression ignoreCondition)
        {
            var dataSourceUseCondition = dataSource.GetConditionOrNull(context);

            if (dataSourceUseCondition != null)
            {
                population.AddCondition(dataSourceUseCondition);
            }

            if (ignoreCondition != null)
            {
                population.AddCondition(ignoreCondition);
            }
        }
    }
}