namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal class ObjectMapperKey
    {
        private readonly MappingRuleSet _ruleSet;
        private readonly IQualifiedMember _sourceMember;
        private readonly QualifiedMember _targetMember;
        private IMappingData _instanceData;
        private Func<IMappingData, bool> _sourceMemberTypeTester;

        private ObjectMapperKey(
            MappingRuleSet ruleSet,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            IMappingData instanceData)
        {
            _ruleSet = ruleSet;
            _sourceMember = sourceMember;
            _targetMember = targetMember;
            _instanceData = instanceData;
        }

        #region Factory Method

        public static ObjectMapperKey For<TSource, TTarget>(
            ObjectMapperDataBridge<TSource, TTarget> bridge)
        {
            return new ObjectMapperKey(
                bridge.MappingContext.RuleSet,
                bridge.SourceMember,
                bridge.TargetMember,
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
                (otherKey._sourceMember == _sourceMember) &&
                (otherKey._targetMember == _targetMember))
            {
                return (_sourceMemberTypeTester == null) ||
                    ((otherKey._instanceData != null) && _sourceMemberTypeTester.Invoke(otherKey._instanceData));
            }

            return false;
        }

        public override int GetHashCode() => 0;
    }
}