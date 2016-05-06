namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    internal class DefaultNullNestedAccessStrategy : INullNestedAccessStrategy
    {
        public static readonly INullNestedAccessStrategy Instance = new DefaultNullNestedAccessStrategy();

        public IMemberPopulation Process(IMemberPopulation population)
        {
            var allNestedAccessesNotNullCheck = population
                .NestedAccesses
                .GetIsNotDefaultComparisons();

            return Update(population, allNestedAccessesNotNullCheck);
        }

        protected virtual IMemberPopulation Update(IMemberPopulation population, Expression nestedAccessesCheck)
        {
            return population.AddCondition(nestedAccessesCheck);
        }
    }
}