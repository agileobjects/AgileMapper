namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class BasicMapperData : IBasicMapperData
    {
        private readonly IBasicMapperData _parent;

        public BasicMapperData(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            IBasicMapperData parent)
            : this(
                parent.RuleSet,
                parent.SourceType,
                parent.TargetType,
                sourceMember,
                targetMember,
                parent)
        {
        }

        public BasicMapperData(
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            IBasicMapperData parent)
            : this(ruleSet, sourceType, targetType, targetMember, parent)
        {
            SourceMember = sourceMember;
        }

        public BasicMapperData(
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType,
            QualifiedMember targetMember,
            IBasicMapperData parent = null)
        {
            IsRoot = parent == null;
            _parent = parent;
            SourceType = sourceType;
            TargetType = targetType;
            RuleSet = ruleSet;
            TargetMember = targetMember ?? QualifiedMember.All;
        }

        IBasicMapperData IBasicMapperData.Parent => _parent;

        public bool IsRoot { get; }

        public MappingRuleSet RuleSet { get; }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public IQualifiedMember SourceMember { get; }

        public QualifiedMember TargetMember { get; }

        public virtual bool HasCompatibleTypes(ITypePair typePair)
            => typePair.HasCompatibleTypes(this, SourceMember, TargetMember);
    }
}