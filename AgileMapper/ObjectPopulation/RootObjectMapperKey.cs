namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class RootObjectMapperKey : ObjectMapperKeyBase
    {
        private readonly MappingRuleSet _ruleSet;

        public RootObjectMapperKey(MappingRuleSet ruleSet, MappingTypes mappingTypes)
            : base(mappingTypes)
        {
            _ruleSet = ruleSet;
        }

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