namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    internal class DefaultNullNestedSourceMemberStrategy : INestedSourceMemberStrategy
    {
        public static readonly INestedSourceMemberStrategy Instance = new DefaultNullNestedSourceMemberStrategy();

        public IMemberPopulation Process(IMemberPopulation population)
        {
            var allSourceMembersNotNullCheck = population
                .NestedSourceMemberAccesses
                .GetIsNotDefaultComparisons();

            return Update(population, allSourceMembersNotNullCheck);
        }

        protected virtual IMemberPopulation Update(IMemberPopulation population, Expression sourceMembersCheck)
        {
            return population.AddCondition(sourceMembersCheck);
        }
    }
}