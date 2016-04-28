namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal interface INestedSourceMemberStrategy
    {
        MemberPopulation Process(
            IEnumerable<Expression> accessedNestedSourceMembers,
            IEnumerable<MemberPopulation> populations);
    }
}