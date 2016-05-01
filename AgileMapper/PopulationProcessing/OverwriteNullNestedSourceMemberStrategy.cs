namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class OverwriteNullNestedSourceMemberStrategy : NullNestedSourceMemberStrategyBase
    {
        internal static readonly INestedSourceMemberStrategy Instance = new OverwriteNullNestedSourceMemberStrategy();

        protected override MemberPopulation GetSinglePopulation(
            Expression sourceMembersCheck,
            MemberPopulation population)
        {
            var valueOrDefault = Expression.Condition(
                sourceMembersCheck,
                population.Value,
                Expression.Default(population.Value.Type));

            return population.WithValue(valueOrDefault);
        }

        protected override MemberPopulation GetMultiplePopulation(
            Expression sourceMembersCheck,
            IEnumerable<MemberPopulation> populations)
        {
            var populate = Expression.IfThen(
                sourceMembersCheck,
                Expression.Block(populations.Select(p => p.Population)));

            return populations.First().WithPopulation(populate);
        }
    }
}
