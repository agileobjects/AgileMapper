namespace AgileObjects.AgileMapper
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources.Factories;
    using Extensions.Internal;
    using Members.Population;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.MapperKeys;
    using ObjectPopulation.RepeatedMappings;

    internal class MappingRuleSet
    {
        private Expression _nameConstant;

        public MappingRuleSet(
            string name,
            MappingRuleSetSettings settings,
            EnumerablePopulationStrategy enumerablePopulationStrategy,
            IRepeatMappingStrategy repeatMappingStrategy,
            PopulationGuardFactory populationGuardFactory,
            FallbackDataSourceFactory fallbackDataSourceFactory,
            RootMapperKeyFactory rootMapperKeyFactory)
            : this(name)
        {
            Settings = settings;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            RepeatMappingStrategy = repeatMappingStrategy;
            PopulationGuardFactory = populationGuardFactory;
            FallbackDataSourceFactory = fallbackDataSourceFactory;
            RootMapperKeyFactory = rootMapperKeyFactory;
        }

        public MappingRuleSet(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Expression NameConstant => _nameConstant ?? (_nameConstant = Name.ToConstantExpression());

        public MappingRuleSetSettings Settings { get; }

        public EnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IRepeatMappingStrategy RepeatMappingStrategy { get; }

        public PopulationGuardFactory PopulationGuardFactory { get; }

        public FallbackDataSourceFactory FallbackDataSourceFactory { get; }
        
        public RootMapperKeyFactory RootMapperKeyFactory { get; }
    }
}