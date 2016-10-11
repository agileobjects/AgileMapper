namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal interface IObjectMapperKey
    {
        MappingRuleSet RuleSet { get; }

        IQualifiedMember SourceMember { get; }

        QualifiedMember TargetMember { get; }

        bool SourceHasRequiredTypes(IMappingData data);
    }

    internal class ObjectMapperKey : IObjectMapperKey
    {
        private Func<IMappingData, bool> _sourceMemberTypeTester;

        public ObjectMapperKey(
            MappingRuleSet ruleSet,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember)
        {
            RuleSet = ruleSet;
            SourceMember = sourceMember;
            TargetMember = targetMember;
        }

        public MappingRuleSet RuleSet { get; }

        public IQualifiedMember SourceMember { get; }

        public QualifiedMember TargetMember { get; }

        public void AddSourceMemberTypeTester(Func<IMappingData, bool> tester)
            => _sourceMemberTypeTester = tester;

        public bool SourceHasRequiredTypes(IMappingData data)
            => (_sourceMemberTypeTester == null) || _sourceMemberTypeTester.Invoke(data);

        public override bool Equals(object obj)
        {
            var otherKey = (IObjectMapperKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            if ((otherKey.RuleSet == RuleSet) &&
                (otherKey.SourceMember == SourceMember) &&
                (otherKey.TargetMember == TargetMember))
            {
                return SourceHasRequiredTypes((IMappingData)otherKey);
            }

            return false;
        }

        public override int GetHashCode() => 0;
    }
}