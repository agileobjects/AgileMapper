namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    internal class DefaultNullNestedAccessStrategy : INullNestedAccessStrategy
    {
        public static readonly INullNestedAccessStrategy Instance = new DefaultNullNestedAccessStrategy();

        public virtual IMemberPopulation ProcessSingle(IMemberPopulation singleMemberPopulation)
            => AddCondition(singleMemberPopulation);

        public IMemberPopulation ProcessMultiple(IMemberPopulation multipleMemberPopulation)
            => AddCondition(multipleMemberPopulation);

        private static IMemberPopulation AddCondition(IMemberPopulation population)
            => population.AddCondition(GetNestedAccessesCheck(population));

        protected static Expression GetNestedAccessesCheck(IMemberPopulation population)
        {
            return population.NestedAccesses.GetIsNotDefaultComparisons();
        }
    }
}