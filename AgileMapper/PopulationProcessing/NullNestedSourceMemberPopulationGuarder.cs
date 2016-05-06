namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using ObjectPopulation;

    internal class NullNestedSourceMemberPopulationGuarder : IPopulationProcessor
    {
        internal static readonly IPopulationProcessor Instance = new NullNestedSourceMemberPopulationGuarder();

        public IEnumerable<IMemberPopulation> Process(IEnumerable<IMemberPopulation> populations)
        {
            var guardedPopulations = populations
                .GroupBy(p => string.Join(",", p.NestedSourceMemberAccesses.Select(m => m.ToString())))
                .OrderBy(grp => grp.Key)
                .Select(grp => grp.HasOne()
                    ? grp.First()
                    : new CompositeMemberPopulation(grp.ToArray()))
                .Select(GetGuardedPopulation)
                .ToArray();

            return guardedPopulations;
        }

        private static IMemberPopulation GetGuardedPopulation(IMemberPopulation population)
        {
            if (!population.NestedSourceMemberAccesses.Any())
            {
                return population;
            }

            return population
                .ObjectMappingContext
                .MappingContext
                .RuleSet
                .NullNestedSourceMemberStrategy
                .Process(population);
        }
    }
}