namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class NullNestedSourceMemberPopulationGuarder : IPopulationProcessor
    {
        internal static readonly IPopulationProcessor Instance = new NullNestedSourceMemberPopulationGuarder();

        public IEnumerable<MemberPopulation> Process(IEnumerable<MemberPopulation> populations)
        {
            var guardedPopulations = populations
                .GroupBy(p => string.Join(",", p.NestedSourceMemberAccesses.Select(m => m.ToString())))
                .OrderBy(grp => grp.Key)
                .SelectMany(grp => (grp.Key == string.Empty)
                    ? grp.ToArray()
                    : GroupedAndGuardedPopulationData(
                        grp.First().NestedSourceMemberAccesses,
                        grp.ToArray()))
                .ToArray();

            return guardedPopulations;
        }

        private static IEnumerable<MemberPopulation> GroupedAndGuardedPopulationData(
            IEnumerable<Expression> accessedNestedSourceMembers,
            IEnumerable<MemberPopulation> populations)
        {
            yield return populations
                .First()
                .ObjectMappingContext.MappingContext.RuleSet
                .NullNestedSourceMemberStrategy
                .Process(accessedNestedSourceMembers, populations);
        }
    }
}