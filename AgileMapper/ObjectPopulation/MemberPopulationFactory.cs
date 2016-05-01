namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using DataSources;
    using Members;

    internal static class MemberPopulationFactory
    {
        public static IEnumerable<MemberPopulation> Create(IObjectMappingContext omc)
        {
            return omc
                .GlobalContext
                .MemberFinder
                .GetTargetMembers(omc.ExistingObject.Type)
                .Select(targetMember => Create(targetMember, omc));
        }

        private static MemberPopulation Create(Member targetMember, IObjectMappingContext omc)
        {
            var qualifiedMember = omc.TargetMember.Append(targetMember);

            if (TargetMemberIsIgnored(qualifiedMember, omc))
            {
                return MemberPopulation.IgnoredMember(targetMember, omc);
            }

            var dataSource = omc
                .MapperContext
                .DataSources
                .FindFor(qualifiedMember, omc);

            return (dataSource != null)
                ? new MemberPopulation(targetMember, dataSource, omc)
                : MemberPopulation.NoDataSource(targetMember, omc);
        }

        private static bool TargetMemberIsIgnored(QualifiedMember qualifiedMember, IObjectMappingContext omc)
        {
            var configurationContext = new ConfigurationContext(qualifiedMember, omc);

            return omc.MapperContext.UserConfigurations.IsIgnored(configurationContext);
        }
    }
}