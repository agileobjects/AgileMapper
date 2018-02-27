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

        private RootObjectMapperKey(MappingRuleSet ruleSet, MappingTypes mappingTypes, MapperContext mapperContext)
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
    }
}