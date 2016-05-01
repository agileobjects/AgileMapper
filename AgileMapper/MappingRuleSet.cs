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
            INestedSourceMemberStrategy nullNestedSourceMemberStrategy,
            IEnumerable<IPopulationProcessor> populationProcessors)
        {
            Name = name;
            ComplexTypeMappingShortCircuitStrategy = complexTypeMappingShortCircuitStrategy;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            NullNestedSourceMemberStrategy = nullNestedSourceMemberStrategy;
            _populationProcessors = populationProcessors;
        }

        public string Name { get; }

        public IComplexTypeMappingShortCircuitStrategy ComplexTypeMappingShortCircuitStrategy { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public INestedSourceMemberStrategy NullNestedSourceMemberStrategy { get; }

        public IEnumerable<MemberPopulation> Process(IEnumerable<MemberPopulation> populations)
        {
            var processedPopulationData = _populationProcessors
                .Aggregate(
                    populations,
                    (populationDataSoFar, processor) => processor.Process(populationDataSoFar));

            return processedPopulationData;
        }
    }
}