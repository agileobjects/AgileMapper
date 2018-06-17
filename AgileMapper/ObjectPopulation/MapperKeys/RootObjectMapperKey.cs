namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using Members.Sources;
    using ReadableExpressions.Extensions;

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
        
        public bool Equals(IRootMapperKey otherKey) => base.Equals(otherKey);

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