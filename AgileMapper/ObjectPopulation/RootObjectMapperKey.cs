namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;
    using Members.Sources;
#if DEBUG
    using ReadableExpressions.Extensions;
#endif

    internal class RootObjectMapperKey : ObjectMapperKeyBase, IRootMapperKey
    {
        private readonly MapperContext _mapperContext;

        public RootObjectMapperKey(MappingTypes mappingTypes, IMappingContext mappingContext)
            : this(mappingContext.RuleSet, mappingTypes, mappingContext.MapperContext)
        {
        }

        public RootObjectMapperKey(MappingRuleSet ruleSet, MappingTypes mappingTypes, MapperContext mapperContext)
            : base(mappingTypes)
        {
            _mapperContext = mapperContext;
            RuleSet = ruleSet;
        }

        public MappingRuleSet RuleSet { get; }

        public override IMembersSource GetMembersSource(ObjectMapperData parentMapperData)
            => _mapperContext.RootMembersSource;

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new RootObjectMapperKey(RuleSet, newMappingTypes, _mapperContext);

        public override bool Equals(object obj)
        {
            var otherKey = (IRootMapperKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            return (otherKey.RuleSet == RuleSet) && Equals(otherKey);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;

        #region ToString
#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var sourceTypeName = MappingTypes.SourceType.GetFriendlyName();
            var targetTypeName = MappingTypes.TargetType.GetFriendlyName();

            return $"{RuleSet.Name}: {sourceTypeName} -> {targetTypeName}";
        }
#endif
        #endregion

        public static class Cache<TSource, TTarget>
        {
            // ReSharper disable StaticMemberInGenericType
            private static ObjectMapperKeyBase _createNew;
            private static ObjectMapperKeyBase _overwrite;
            private static ObjectMapperKeyBase _project;
            private static ObjectMapperKeyBase _merge;
            // ReSharper restore StaticMemberInGenericType

            private static ObjectMapperKeyBase CreateNew
                => _createNew ?? (_createNew = CreateKey(MappingRuleSetCollection.Default.CreateNew));

            private static ObjectMapperKeyBase Overwrite
                => _overwrite ?? (_overwrite = CreateKey(MappingRuleSetCollection.Default.Overwrite));

            private static ObjectMapperKeyBase Project
                => _project ?? (_project = CreateKey(MappingRuleSetCollection.Default.Project));

            private static ObjectMapperKeyBase Merge
                => _merge ?? (_merge = CreateKey(MappingRuleSetCollection.Default.Merge));

            private static ObjectMapperKeyBase CreateKey(MappingRuleSet ruleSet)
            {
                return new RootObjectMapperKey(
                    ruleSet,
                    MappingTypes<TSource, TTarget>.Fixed,
                    mapperContext: null);
            }

            public static ObjectMapperKeyBase GetKeyFor(IMappingContext mappingContext)
            {
                var ruleSets = mappingContext.MapperContext.RuleSets;

                if (mappingContext.RuleSet == ruleSets.CreateNew)
                {
                    return CreateNew;
                }

                if (mappingContext.RuleSet == ruleSets.Overwrite)
                {
                    return Overwrite;
                }

                if (mappingContext.RuleSet == ruleSets.Project)
                {
                    return Project;
                }

                return Merge;
            }
        }
    }
}