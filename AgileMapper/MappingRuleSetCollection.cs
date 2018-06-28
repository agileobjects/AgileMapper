namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using DataSources;
    using Extensions.Internal;
    using Members.Population;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.MapperKeys;
    using ObjectPopulation.Recursion;
    using Queryables;
    using Queryables.Recursion;

    internal class MappingRuleSetCollection
    {
        #region Default Rule Sets

        private static readonly MappingRuleSet _createNew = new MappingRuleSet(
            Constants.CreateNew,
            MappingRuleSetSettings.ForInMemoryMapping(),
            default(CopySourceEnumerablePopulationStrategy),
            default(MapRecursionCallRecursiveMemberMappingStrategy),
            DefaultMemberPopulationFactory.Instance,
            default(ExistingOrDefaultValueDataSourceFactory),
            default(RootMapperKeyFactory));

        private static readonly MappingRuleSet _overwrite = new MappingRuleSet(
            Constants.Overwrite,
            MappingRuleSetSettings.ForInMemoryMapping(rootHasPopulatedTarget: true),
            default(OverwriteEnumerablePopulationStrategy),
            default(MapRecursionCallRecursiveMemberMappingStrategy),
            DefaultMemberPopulationFactory.Instance,
            default(DefaultValueDataSourceFactory),
            default(RootMapperKeyFactory));

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
            default(ProjectSourceEnumerablePopulationStrategy),
            default(MapToDepthRecursiveMemberMappingStrategy),
            DefaultMemberPopulationFactory.Instance,
            default(DefaultValueDataSourceFactory),
            default(QueryProjectorMapperKeyFactory));

        private static readonly MappingRuleSet _merge = new MappingRuleSet(
            Constants.Merge,
            MappingRuleSetSettings.ForInMemoryMapping(rootHasPopulatedTarget: true),
            default(MergeEnumerablePopulationStrategy),
            default(MapRecursionCallRecursiveMemberMappingStrategy),
            new MemberMergePopulationFactory(),
            default(ExistingOrDefaultValueDataSourceFactory),
            default(RootMapperKeyFactory));

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