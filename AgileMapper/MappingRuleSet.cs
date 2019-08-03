namespace AgileObjects.AgileMapper
{
    using DataSources;
    using Extensions.Internal;
    using Members.Population;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.MapperKeys;
    using ObjectPopulation.RepeatedMappings;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MappingRuleSet
    {
        private Expression _nameConstant;

        public MappingRuleSet(
            string name,
            MappingRuleSetSettings settings,
            IEnumerablePopulationStrategy enumerablePopulationStrategy,
            IRepeatMappingStrategy repeatMappingStrategy,
            IMemberPopulationFactory populationFactory,
            IDataSourceFactory fallbackDataSourceFactory,
            IRootMapperKeyFactory rootMapperKeyFactory)
            : this(name)
        {
            Settings = settings;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            RepeatMappingStrategy = repeatMappingStrategy;
            PopulationFactory = populationFactory;
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

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IRepeatMappingStrategy RepeatMappingStrategy { get; }

        public IMemberPopulationFactory PopulationFactory { get; }

        public IDataSourceFactory FallbackDataSourceFactory { get; }
        
        public IRootMapperKeyFactory RootMapperKeyFactory { get; }
    }
}