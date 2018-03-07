namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members.Population;
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.Recursion;
    using Queryables.Recursion;

    internal class MappingRuleSetCollection
    {
        #region Default Rule Sets

        private static readonly MappingRuleSet _createNew = new MappingRuleSet(
            Constants.CreateNew,
            MappingRuleSetSettings.ForInMemoryMapping(),
            new CopySourceEnumerablePopulationStrategy(),
            MapRecursionCallRecursiveMemberMappingStrategy.Instance,
            DefaultMemberPopulationFactory.Instance,
            ExistingOrDefaultValueDataSourceFactory.Instance);

        private static readonly MappingRuleSet _overwrite = new MappingRuleSet(
            Constants.Overwrite,
            MappingRuleSetSettings.ForInMemoryMapping(rootHasPopulatedTarget: true),
            OverwriteEnumerablePopulationStrategy.Instance,
            MapRecursionCallRecursiveMemberMappingStrategy.Instance,
            DefaultMemberPopulationFactory.Instance,
            DefaultValueDataSourceFactory.Instance);

        private static readonly MappingRuleSet _project = new MappingRuleSet(
            Constants.Project,
            new MappingRuleSetSettings
            {
                UseMemberInitialisation = true,
                UseSingleRootMappingExpression = true,
                GuardAccessTo = value => value.Type.IsComplex(),
                ExpressionIsSupported = value => value.CanBeProjected(),
                AllowEnumerableAssignment = true
            },
            new ProjectSourceEnumerablePopulationStrategy(),
            new MapToDepthRecursiveMemberMappingStrategy(),
            DefaultMemberPopulationFactory.Instance,
            DefaultValueDataSourceFactory.Instance);

        private static readonly MappingRuleSet _merge = new MappingRuleSet(
            Constants.Merge,
            MappingRuleSetSettings.ForInMemoryMapping(rootHasPopulatedTarget: true),
            new MergeEnumerablePopulationStrategy(),
            MapRecursionCallRecursiveMemberMappingStrategy.Instance,
            new MemberMergePopulationFactory(),
            ExistingOrDefaultValueDataSourceFactory.Instance);

        public static readonly MappingRuleSetCollection Default =
            new MappingRuleSetCollection(_createNew, _overwrite, _project, _merge);

        #endregion

        public MappingRuleSetCollection(params MappingRuleSet[] ruleSets)
        {
            All = ruleSets;
        }

        public IList<MappingRuleSet> All { get; }

        public MappingRuleSet CreateNew => _createNew;

        public MappingRuleSet Merge => _merge;

        public MappingRuleSet Overwrite => _overwrite;

        public MappingRuleSet Project => _project;

        public MappingRuleSet GetByName(string name) => All.First(rs => rs.Name == name);
    }
}