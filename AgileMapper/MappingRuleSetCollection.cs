namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using DataSources;
    using Members;
    using ObjectPopulation;

    internal class MappingRuleSetCollection
    {
        private readonly List<MappingRuleSet> _ruleSets;

        public MappingRuleSetCollection()
        {
            CreateNew = new MappingRuleSet(
                Constants.CreateNew,
                CopySourceEnumerablePopulationStrategy.Instance,
                NullDataSourceFactory.Instance,
                ExistingOrDefaultValueDataSourceFactory.Instance);

            Merge = new MappingRuleSet(
                Constants.Merge,
                MergeEnumerablePopulationStrategy.Instance,
                PreserveExistingValueDataSourceFactory.Instance,
                ExistingOrDefaultValueDataSourceFactory.Instance);

            Overwrite = new MappingRuleSet(
                Constants.Overwrite,
                OverwriteEnumerablePopulationStrategy.Instance,
                NullDataSourceFactory.Instance,
                DefaultValueDataSourceFactory.Instance);

            _ruleSets = new List<MappingRuleSet> { CreateNew, Merge, Overwrite };
        }

        public MappingRuleSet CreateNew { get; }

        public MappingRuleSet Merge { get; }

        public MappingRuleSet Overwrite { get; set; }

        public MappingRuleSet GetByName(string name) => _ruleSets.First(rs => rs.Name == name);
    }
}