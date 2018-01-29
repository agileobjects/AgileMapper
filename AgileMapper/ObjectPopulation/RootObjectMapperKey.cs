namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;
    using Members.Sources;
#if DEBUG
    using ReadableExpressions.Extensions;
#endif

    internal class RootObjectMapperKey : ObjectMapperKeyBase
    {
        private readonly MapperContext _mapperContext;
        private readonly MappingRuleSet _ruleSet;

        public RootObjectMapperKey(MappingTypes mappingTypes, IMappingContext mappingContext)
            : this(mappingContext.RuleSet, mappingTypes, mappingContext.MapperContext)
        {
        }

        private RootObjectMapperKey(MappingRuleSet ruleSet, MappingTypes mappingTypes, MapperContext mapperContext)
            : base(mappingTypes)
        {
            _mapperContext = mapperContext;
            _ruleSet = ruleSet;
        }

        public override IMembersSource GetMembersSource(IObjectMappingData parentMappingData)
            => _mapperContext.RootMembersSource;

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new RootObjectMapperKey(_ruleSet, newMappingTypes, _mapperContext);

        public override bool Equals(object obj)
        {
            var otherKey = (RootObjectMapperKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            return TypesMatch(otherKey) &&
                  (otherKey._ruleSet == _ruleSet) &&
                   SourceHasRequiredTypes(otherKey);
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

            return $"{_ruleSet.Name}: {sourceTypeName} -> {targetTypeName}";
        }
#endif
        #endregion
    }
}