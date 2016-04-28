namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    internal abstract class NullNestedSourceMemberStrategyBase : INestedSourceMemberStrategy
    {
        public MemberPopulation Process(
            IEnumerable<Expression> accessedNestedSourceMembers,
            IEnumerable<MemberPopulation> populations)
        {
            var allSourceMembersNotNullCheck =
                GetAllSourceMembersNotNullCheck(accessedNestedSourceMembers);

            return populations.HasOne()
                ? GetSinglePopulation(allSourceMembersNotNullCheck, populations.First())
                : GetMultiplePopulation(allSourceMembersNotNullCheck, populations);
        }

        private static Expression GetAllSourceMembersNotNullCheck(IEnumerable<Expression> accessedNestedSourceMembers)
        {
            if (accessedNestedSourceMembers.Count() == 1)
            {
                return accessedNestedSourceMembers.First().GetIsNotDefaultComparison();
            }

            var sourceMemberNotNullChecks = accessedNestedSourceMembers
                .Select(sourceMember => sourceMember.GetIsNotDefaultComparison())
                .ToArray();

            var allSourceMembersNotNullCheck = sourceMemberNotNullChecks
                .Skip(1)
                .Aggregate(sourceMemberNotNullChecks.First(), Expression.AndAlso);

            return allSourceMembersNotNullCheck;
        }

        protected abstract MemberPopulation GetSinglePopulation(
            Expression sourceMembersCheck,
            MemberPopulation population);

        protected abstract MemberPopulation GetMultiplePopulation(
            Expression sourceMembersCheck,
            IEnumerable<MemberPopulation> populations);
    }
}