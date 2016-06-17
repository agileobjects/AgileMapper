namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal static class MemberPopulationFactory
    {
        public static IEnumerable<IMemberPopulation> Create(IObjectMappingContext omc)
        {
            return omc
                .GlobalContext
                .MemberFinder
                .GetWriteableMembers(omc.ExistingObject.Type)
                .Select(targetMember => Create(targetMember, omc));
        }

        private static IMemberPopulation Create(Member targetMember, IObjectMappingContext omc)
        {
            var qualifiedMember = omc.TargetMember.Append(targetMember);
            var context = new MemberMappingContext(qualifiedMember, omc);

            Expression populateCondition;

            if (TargetMemberPopulationIsConditional(context, out populateCondition))
            {
                return MemberPopulation.IgnoredMember(context);
            }

            var dataSources = context.GetDataSources();

            if (dataSources.None)
            {
                return MemberPopulation.NoDataSource(context);
            }

            return new MemberPopulation(context, dataSources, populateCondition);
        }

        private static bool TargetMemberPopulationIsConditional(
            IMemberMappingContext context,
            out Expression populateCondition)
        {
            var configuredIgnore = context
                .MapperContext
                .UserConfigurations
                .GetMemberIgnoreOrNull(context);

            if (configuredIgnore == null)
            {
                populateCondition = null;
                return false;
            }

            populateCondition = configuredIgnore.GetConditionOrNull(context);
            return (populateCondition == null);
        }
    }
}