namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Diagnostics;
    using Members;
    using Members.Sources;

    [DebuggerDisplay("{_ruleSet.Name}: {MappingTypes.SourceType.Name} -> {MappingTypes.TargetType.Name}")]
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
            if ((otherKey._ruleSet == _ruleSet) && TypesMatch(otherKey))
            {
                return SourceHasRequiredTypes(otherKey);
            }

            return false;
        }

        public override int GetHashCode() => 0;
    }
}