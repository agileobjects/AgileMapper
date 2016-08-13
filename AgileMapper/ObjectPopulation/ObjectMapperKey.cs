namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class ObjectMapperKey
    {
        private readonly MappingRuleSet _ruleSet;
        private readonly string _sourceMemberSignature;
        private readonly string _targetMemberSignature;

        private ObjectMapperKey(
            MappingRuleSet ruleSet,
            string sourceMemberSignature,
            string targetMemberSignature)
        {
            _ruleSet = ruleSet;
            _sourceMemberSignature = sourceMemberSignature;
            _targetMemberSignature = targetMemberSignature;
        }

        #region Factory Method

        public static ObjectMapperKey For<TSource, TTarget>(
            ObjectMapperDataBridge<TSource, TTarget> bridge)
        {
            return For(
                bridge.MappingContext,
                bridge.SourceMember,
                bridge.TargetMember);
        }

        public static ObjectMapperKey For(
            MappingContext mappingContext,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember)
        {
            return new ObjectMapperKey(
                mappingContext.RuleSet,
                sourceMember.Signature,
                targetMember.Signature);
        }

        #endregion

        public override bool Equals(object obj)
        {
            var otherKey = (ObjectMapperKey)obj;

            return (otherKey._ruleSet == _ruleSet) &&
                   (otherKey._sourceMemberSignature == _sourceMemberSignature) &&
                   (otherKey._targetMemberSignature == _targetMemberSignature);
        }

        public override int GetHashCode() => 0;
    }
}