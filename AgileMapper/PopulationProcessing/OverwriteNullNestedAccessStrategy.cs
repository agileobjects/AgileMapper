namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class OverwriteNullNestedAccessStrategy : DefaultNullNestedAccessStrategy
    {
        internal new static readonly INullNestedAccessStrategy Instance = new OverwriteNullNestedAccessStrategy();

        protected override IMemberPopulation Update(IMemberPopulation population, Expression nestedAccessesCheck)
        {
            if (population.IsMultiplePopulation)
            {
                return base.Update(population, nestedAccessesCheck);
            }

            var valueOrDefault = Expression.Condition(
                nestedAccessesCheck,
                population.Value,
                Expression.Default(population.Value.Type));

            return population.WithValue(valueOrDefault);
        }
    }
}
