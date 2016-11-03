namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class BasicMapperData : IBasicMapperData
    {
        private readonly IBasicMapperData _parent;

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

        public static BasicMapperData WithNoTargetMember(IMemberMapperData parent)
        {
            return new BasicMapperData(
                parent.RuleSet,
                parent.SourceType,
                parent.TargetType,
                QualifiedMember.None,
                parent);
        }

        IBasicMapperData IBasicMapperData.Parent => _parent;

        public bool IsRoot { get; }

        public MappingRuleSet RuleSet { get; }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public QualifiedMember TargetMember { get; }
    }
}