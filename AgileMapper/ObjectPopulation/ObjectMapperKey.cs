namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal class ObjectMapperKey
    {
        private readonly MappingRuleSet _ruleSet;
        private readonly string _sourceMemberSignature;
        private readonly string _targetMemberSignature;
        private IMappingData _instanceData;
        private Func<IMappingData, bool> _sourceMemberTypeTester;

        private ObjectMapperKey(
            MappingRuleSet ruleSet,
            string sourceMemberSignature,
            string targetMemberSignature,
            IMappingData instanceData)
        {
            _ruleSet = ruleSet;
            _sourceMemberSignature = sourceMemberSignature;
            _targetMemberSignature = targetMemberSignature;
            _instanceData = instanceData;
        }

        #region Factory Method

        public static ObjectMapperKey For<TSource, TTarget>(
            ObjectMapperDataBridge<TSource, TTarget> bridge)
        {
            return new ObjectMapperKey(
                bridge.MappingContext.RuleSet,
                bridge.SourceMember.Signature,
                bridge.TargetMember.Signature,
                bridge.InstanceData);
        }

        #endregion

        public void AddSourceMemberTypeTester(Func<IMappingData, bool> tester)
        {
            _sourceMemberTypeTester = tester;
        }

        internal void RemoveInstanceData()
        {
            _instanceData = null;
        }

        public override bool Equals(object obj)
        {
            var otherKey = (ObjectMapperKey)obj;

            if ((otherKey._ruleSet == _ruleSet) &&
                (otherKey._sourceMemberSignature == _sourceMemberSignature) &&
                (otherKey._targetMemberSignature == _targetMemberSignature))
            {
                return (_sourceMemberTypeTester == null) || _sourceMemberTypeTester.Invoke(otherKey._instanceData);
            }

            return false;
        }

        public override int GetHashCode() => 0;
    }
}