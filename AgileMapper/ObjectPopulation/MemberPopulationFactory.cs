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
                .Select(targetMember => Create(targetMember, omc));
        }

        private static IMemberPopulation Create(Member targetMember, IObjectMappingContext omc)
        {
            var qualifiedMember = omc.TargetMember.Append(targetMember);

            Expression ignoreCondition;

            if (TargetMemberIsIgnored(qualifiedMember, omc, out ignoreCondition) &&
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

            if (ignoreCondition != null)
            {
                population.AddCondition(ignoreCondition);
            }

            return population;
        }

        private static bool TargetMemberIsIgnored(
            QualifiedMember qualifiedMember,
            IObjectMappingContext omc,
            out Expression ignoreCondition)
        {
            var configurationContext = new ConfigurationContext(qualifiedMember, omc);

            return omc.MapperContext.UserConfigurations.IsIgnored(configurationContext, out ignoreCondition);
        }
    }
}