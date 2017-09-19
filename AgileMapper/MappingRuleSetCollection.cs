namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using Extensions;
    using Members.Population;
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;

    internal class MappingRuleSetCollection
    {
        #region Default Rule Sets

        private static readonly MappingRuleSet _createNew = new MappingRuleSet(
            Constants.CreateNew,
            false,
            CopySourceEnumerablePopulationStrategy.Instance,
            NullMemberPopulationGuardFactory.Instance,
            ExistingOrDefaultValueDataSourceFactory.Instance);

        private static readonly MappingRuleSet _merge = new MappingRuleSet(
            Constants.Merge,
            true,
            MergeEnumerablePopulationStrategy.Instance,
            PreserveExistingValueMemberPopulationGuardFactory.Instance,
            ExistingOrDefaultValueDataSourceFactory.Instance);

        private static readonly MappingRuleSet _overwrite = new MappingRuleSet(
            Constants.Overwrite,
            true,
            OverwriteEnumerablePopulationStrategy.Instance,
            NullMemberPopulationGuardFactory.Instance,
            DefaultValueDataSourceFactory.Instance);

        #endregion

        private readonly List<MappingRuleSet> _ruleSets;

        public MappingRuleSetCollection()
        {
            _ruleSets = new List<MappingRuleSet> { CreateNew, Merge, Overwrite };
        }

        public MappingRuleSet CreateNew => _createNew;

        public MappingRuleSet Merge => _merge;

        public MappingRuleSet Overwrite => _overwrite;

        public MappingRuleSet GetByName(string name) => _ruleSets.First(rs => rs.Name == name);
    }
}