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
            IEnumerable<MemberPopulation> populationData)
        {
            var populations = populationData
                .Select(p => new
                {
                    p.Population,
                    SetToDefault = p.ObjectMappingContext
                        .GetPopulation(p.TargetMember, Expression.Default(p.TargetMember.Type))
                })
                .ToArray();

            var populateOrSetToDefault = Expression.IfThenElse(
                sourceMembersCheck,
                Expression.Block(populations.Select(p => p.Population).ToArray()),
                Expression.Block(populations.Select(p => p.SetToDefault).ToArray()));

            return MemberPopulation.Empty.WithPopulation(populateOrSetToDefault);
        }
    }
}
