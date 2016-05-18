namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
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
            var context = new MemberMappingContext(qualifiedMember, omc);

            Expression ignoreCondition;

            if (TargetMemberIsAlwaysIgnored(context, out ignoreCondition))
            {
                return MemberPopulation.IgnoredMember(context);
            }

            var dataSources = context.GetDataSources();

            if (dataSources.None())
            {
                return MemberPopulation.NoDataSource(context);
            }

            return new MemberPopulation(context, dataSources).WithCondition(ignoreCondition);
        }

        private static bool TargetMemberIsAlwaysIgnored(IMemberMappingContext context, out Expression ignoreCondition)
        {
            var isIgnorable = context.Parent.MapperContext.UserConfigurations.IsIgnored(context, out ignoreCondition);

            return isIgnorable && (ignoreCondition == null);
        }
    }
}