namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class DefaultNullNestedSourceMemberStrategy : NullNestedSourceMemberStrategyBase
    {
        public static readonly INestedSourceMemberStrategy Instance = new DefaultNullNestedSourceMemberStrategy();

        protected override MemberPopulation GetSinglePopulation(
            Expression sourceMembersCheck,
            MemberPopulation population)
        {
            return GetGuardedPopulation(sourceMembersCheck, population, population.Population);
        }

        protected override MemberPopulation GetMultiplePopulation(
            Expression sourceMembersCheck,
            IEnumerable<MemberPopulation> populations)
        {
            return GetGuardedPopulation(
                sourceMembersCheck,
                MemberPopulation.Empty,
                Expression.Block(populations.Select(d => d.Population)));
        }

        private static MemberPopulation GetGuardedPopulation(
            Expression sourceMembersCheck,
            MemberPopulation population,
            Expression populationExpression)
        {
            var guardedPopulation = Expression.IfThen(sourceMembersCheck, populationExpression);

            return population.WithPopulation(guardedPopulation);
        }
    }
}