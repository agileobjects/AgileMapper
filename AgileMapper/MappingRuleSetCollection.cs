namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using Extensions;
    using Members.Population;
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;

    internal class MappingRuleSetCollection
    {
        private readonly List<MappingRuleSet> _ruleSets;

        public MappingRuleSetCollection()
        {
            CreateNew = new MappingRuleSet(
                Constants.CreateNew,
                false,
                CopySourceEnumerablePopulationStrategy.Instance,
                NullMemberPopulationGuardFactory.Instance,
                ExistingOrDefaultValueDataSourceFactory.Instance);

            Merge = new MappingRuleSet(
                Constants.Merge,
                true,
                MergeEnumerablePopulationStrategy.Instance,
                PreserveExistingValueMemberPopulationGuardFactory.Instance,
                ExistingOrDefaultValueDataSourceFactory.Instance);

            Overwrite = new MappingRuleSet(
                Constants.Overwrite,
                true,
                OverwriteEnumerablePopulationStrategy.Instance,
                NullMemberPopulationGuardFactory.Instance,
                DefaultValueDataSourceFactory.Instance);

            _ruleSets = new List<MappingRuleSet> { CreateNew, Merge, Overwrite };
        }

        public MappingRuleSet CreateNew { get; }

        public MappingRuleSet Merge { get; }

        public MappingRuleSet Overwrite { get; set; }

        public MappingRuleSet GetByName(string name) => _ruleSets.First(rs => rs.Name == name);
    }
}