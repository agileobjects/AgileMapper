namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using ObjectPopulation;
    using PopulationProcessing;

    internal class MappingRuleSet
    {
        private readonly IEnumerable<IPopulationProcessor> _populationProcessors;

        public MappingRuleSet(
            string name,
            IComplexTypeMappingShortCircuitStrategy complexTypeMappingShortCircuitStrategy,
            IEnumerablePopulationStrategy enumerablePopulationStrategy,
            INullNestedAccessStrategy nullNestedAccessStrategy,
            IEnumerable<IPopulationProcessor> populationProcessors)
        {
            Name = name;
            ComplexTypeMappingShortCircuitStrategy = complexTypeMappingShortCircuitStrategy;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            NullNestedAccessStrategy = nullNestedAccessStrategy;
            _populationProcessors = populationProcessors;
        }

        public string Name { get; }

        public IComplexTypeMappingShortCircuitStrategy ComplexTypeMappingShortCircuitStrategy { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public INullNestedAccessStrategy NullNestedAccessStrategy { get; }

        public IEnumerable<IMemberPopulation> Process(IEnumerable<IMemberPopulation> populations)
        {
            var processedPopulationData = _populationProcessors
                .Aggregate(
                    populations,
                    (populationDataSoFar, processor) => processor.Process(populationDataSoFar));

            return processedPopulationData;
        }
    }
}