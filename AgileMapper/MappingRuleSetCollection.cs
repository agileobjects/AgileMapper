namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using DataSources.Factories;
    using Extensions.Internal;
    using Members.Population;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.MapperKeys;
    using ObjectPopulation.RepeatedMappings;
    using Queryables;
    using Queryables.Recursion;

    internal class MappingRuleSetCollection
    {
        #region Default Rule Sets

        private static readonly MappingRuleSet _createNew = new MappingRuleSet(
            Constants.CreateNew,
            MappingRuleSetSettings.ForInMemoryMapping(allowCloneEntityKeyMapping: true),
            CopySourceEnumerablePopulationStrategy.Create,
            default(MapRepeatedCallRepeatMappingStrategy),
            NullMemberPopulationGuardFactory.Create,
            ExistingOrDefaultValueFallbackDataSourceFactory.Create,
            DefaultRootMapperKeyFactory.Create);

        private static readonly MappingRuleSet _overwrite = new MappingRuleSet(
            Constants.Overwrite,
            MappingRuleSetSettings.ForInMemoryMapping(rootHasPopulatedTarget: true),
            OverwriteEnumerablePopulationStrategy.Create,
            default(MapRepeatedCallRepeatMappingStrategy),
            NullMemberPopulationGuardFactory.Create,
            DefaultValueFallbackDataSourceFactory.Create,
            DefaultRootMapperKeyFactory.Create);

        private static readonly MappingRuleSet _project = new MappingRuleSet(
            Constants.Project,
            new MappingRuleSetSettings
            {
                UseMemberInitialisation = true,
                UseSingleRootMappingExpression = true,
                AllowEntityKeyMapping = true,
                AllowCloneEntityKeyMapping = true,
                AllowGuardedBindings = true,
                GuardAccessTo = value => value.Type.IsComplex(),
                ExpressionIsSupported = value => value.CanBeProjected(),
                AllowEnumerableAssignment = true
            },
            ProjectSourceEnumerablePopulationStrategy.Create,
            default(MapToDepthRepeatMappingStrategy),
            NullMemberPopulationGuardFactory.Create,
            DefaultValueFallbackDataSourceFactory.Create,
            QueryProjectorMapperKeyFactory.Create);

        private static readonly MappingRuleSet _merge = new MappingRuleSet(
            Constants.Merge,
            MappingRuleSetSettings.ForInMemoryMapping(rootHasPopulatedTarget: true, allowGuardedBindings: false),
            MergeEnumerablePopulationStrategy.Create,
            default(MapRepeatedCallRepeatMappingStrategy),
            MemberMergePopulationGuardFactory.Create,
            ExistingOrDefaultValueFallbackDataSourceFactory.Create,
            DefaultRootMapperKeyFactory.Create);

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