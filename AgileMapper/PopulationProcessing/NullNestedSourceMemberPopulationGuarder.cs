namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ObjectPopulation;

    internal class NullNestedSourceMemberPopulationGuarder : IPopulationProcessor
    {
        internal static readonly IPopulationProcessor Instance = new NullNestedSourceMemberPopulationGuarder();

        public IEnumerable<IMemberPopulation> Process(IEnumerable<IMemberPopulation> populations)
        {
            var guardedPopulations = populations
                .GroupBy(p => string.Join(",", p.NestedAccesses.Select(m => m.ToString())))
                .Select(grp => grp.ToArray())
                .Select(groupedPopulations => (groupedPopulations.Length == 1)
                    ? GetSingleGuardedPopulation(groupedPopulations[0])
                    : GetMultipleGuardedPopulation(groupedPopulations))
                .ToArray();

            return guardedPopulations;
        }

        private static IMemberPopulation GetSingleGuardedPopulation(IMemberPopulation population)
        {
            return GetGuardedPopulation(population, s => s.ProcessSingle);
        }

        private static IMemberPopulation GetMultipleGuardedPopulation(IEnumerable<IMemberPopulation> populations)
        {
            var composite = new CompositeMemberPopulation(populations);

            return GetGuardedPopulation(composite, s => s.ProcessMultiple);
        }

        private static IMemberPopulation GetGuardedPopulation(
            IMemberPopulation population,
            Func<INullNestedAccessStrategy, Func<IMemberPopulation, IMemberPopulation>> processorFactory)
        {
            if (!population.NestedAccesses.Any())
            {
                return population;
            }

            var processor = processorFactory.Invoke(population
                .ObjectMappingContext
                .MappingContext
                .RuleSet
                .NullNestedAccessStrategy);

            return processor.Invoke(population);
        }
    }
}