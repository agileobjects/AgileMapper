namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class OverwriteNullNestedSourceMemberStrategy : DefaultNullNestedSourceMemberStrategy
    {
        internal new static readonly INestedSourceMemberStrategy Instance = new OverwriteNullNestedSourceMemberStrategy();

        protected override IMemberPopulation Update(IMemberPopulation population, Expression sourceMembersCheck)
        {
            if (population.IsMultiplePopulation)
            {
                return base.Update(population, sourceMembersCheck);
            }

            var valueOrDefault = Expression.Condition(
                sourceMembersCheck,
                population.Value,
                Expression.Default(population.Value.Type));

            return population.WithValue(valueOrDefault);
        }
    }
}
