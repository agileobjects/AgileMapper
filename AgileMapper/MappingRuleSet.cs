namespace AgileObjects.AgileMapper
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions.Internal;
    using Members.Population;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.Recursion;

    internal class MappingRuleSet
    {
        private Expression _nameConstant;

        public MappingRuleSet(
            string name,
            MappingRuleSetSettings settings,
            IEnumerablePopulationStrategy enumerablePopulationStrategy,
            IRecursiveMemberMappingStrategy recursiveMemberMappingStrategy,
            IMemberPopulationFactory populationFactory,
            IDataSourceFactory fallbackDataSourceFactory)
        {
            Name = name;
            Settings = settings;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            RecursiveMemberMappingStrategy = recursiveMemberMappingStrategy;
            PopulationFactory = populationFactory;
            FallbackDataSourceFactory = fallbackDataSourceFactory;
        }

        public string Name { get; }

        public Expression NameConstant => _nameConstant ?? (_nameConstant = Name.ToConstantExpression());

        public MappingRuleSetSettings Settings { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IRecursiveMemberMappingStrategy RecursiveMemberMappingStrategy { get; }

        public IMemberPopulationFactory PopulationFactory { get; }

        public IDataSourceFactory FallbackDataSourceFactory { get; }
    }
}