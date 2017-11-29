namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using Extensions;
    using Members.Population;
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.Recursion;

    internal class MappingRuleSetCollection
    {
        #region Default Rule Sets

        private static readonly MappingRuleSet _createNew = new MappingRuleSet(
            Constants.CreateNew,
            new MappingRuleSetSettings
            {
                SourceElementsCouldBeNull = true,
                UseTryCatch = true,
                GuardMemberAccesses = true,
                AllowObjectTracking = true
            },
            new CopySourceEnumerablePopulationStrategy(),
            MapRecursionCallRecursiveMemberMappingStrategy.Instance,
            NullMemberPopulationGuardFactory.Instance,
            ExistingOrDefaultValueDataSourceFactory.Instance);

        private static readonly MappingRuleSet _merge = new MappingRuleSet(
            Constants.Merge,
            new MappingRuleSetSettings
            {
                RootHasPopulatedTarget = true,
                SourceElementsCouldBeNull = true,
                UseTryCatch = true,
                GuardMemberAccesses = true,
                AllowObjectTracking = true
            },
            new MergeEnumerablePopulationStrategy(),
            MapRecursionCallRecursiveMemberMappingStrategy.Instance,
            new PreserveExistingValueMemberPopulationGuardFactory(),
            ExistingOrDefaultValueDataSourceFactory.Instance);

        private static readonly MappingRuleSet _overwrite = new MappingRuleSet(
            Constants.Overwrite,
            new MappingRuleSetSettings
            {
                RootHasPopulatedTarget = true,
                SourceElementsCouldBeNull = true,
                UseTryCatch = true,
                GuardMemberAccesses = true,
                AllowObjectTracking = true
            },
            OverwriteEnumerablePopulationStrategy.Instance,
            MapRecursionCallRecursiveMemberMappingStrategy.Instance,
            NullMemberPopulationGuardFactory.Instance,
            DefaultValueDataSourceFactory.Instance);

        private static readonly MappingRuleSet _project = new MappingRuleSet(
            Constants.Project,
            new MappingRuleSetSettings
            {
                UseMemberInitialisation = true,
                UseSingleRootMappingExpression = true,
                AllowEnumerableAssignment = true
            },
            new ProjectSourceEnumerablePopulationStrategy(),
            new MapToDepthRecursiveMemberMappingStrategy(),
            NullMemberPopulationGuardFactory.Instance,
            DefaultValueDataSourceFactory.Instance);

        #endregion

        private readonly List<MappingRuleSet> _ruleSets;

        public MappingRuleSetCollection()
        {
            _ruleSets = new List<MappingRuleSet> { CreateNew, Merge, Overwrite, Project };
        }

        public IEnumerable<MappingRuleSet> All => _ruleSets;

        public MappingRuleSet CreateNew => _createNew;

        public MappingRuleSet Merge => _merge;

        public MappingRuleSet Overwrite => _overwrite;

        public MappingRuleSet Project => _project;

        public MappingRuleSet GetByName(string name) => _ruleSets.First(rs => rs.Name == name);
    }
}