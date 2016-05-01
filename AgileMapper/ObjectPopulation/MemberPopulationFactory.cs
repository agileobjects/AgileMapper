namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
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
            var dataSource = omc
                .MapperContext
                .DataSources
                .FindFor(targetMember, omc);

            return (dataSource != null)
                ? new MemberPopulation(targetMember, dataSource, omc)
                : MemberPopulation.NoDataSource(targetMember, omc);
        }
    }
}