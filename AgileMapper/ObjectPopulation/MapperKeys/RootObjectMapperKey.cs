namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using Members.Sources;
#if DEBUG
    using ReadableExpressions.Extensions;
#endif

    internal class RootObjectMapperKey : ObjectMapperKeyBase, IRootMapperKey
    {
        private readonly IMembersSource _membersSource;

        public RootObjectMapperKey(MappingExecutionContextBase2 context)
            : this(context.RuleSet, context.MappingTypes, context.MapperContext.RootMembersSource)
        {
            KeyData = context;
        }

        public RootObjectMapperKey(MappingRuleSet ruleSet, MappingTypes mappingTypes, IMembersSource membersSource)
            : base(mappingTypes)
        {
            _membersSource = membersSource;
            RuleSet = ruleSet;
        }

        public MappingRuleSet RuleSet { get; }

        public override IMembersSource GetMembersSource(ObjectMapperData parentMapperData) => _membersSource;

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new RootObjectMapperKey(RuleSet, newMappingTypes, _membersSource);

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