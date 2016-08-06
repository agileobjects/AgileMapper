namespace AgileObjects.AgileMapper
{
    using System;
    using Members;

    internal class BasicMappingData : IMappingData
    {
        public BasicMappingData(
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType,
            QualifiedMember targetMember = null)
        {
            SourceType = sourceType;
            TargetType = targetType;
            RuleSetName = ruleSet.Name;
            TargetMember = targetMember ?? QualifiedMember.All;
        }

        private BasicMappingData(
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType,
            QualifiedMember targetMember,
            IMappingData parent)
            : this(ruleSet, sourceType, targetType, targetMember)
        {
            Parent = parent;
        }

        public static IMappingData WithNoTargetMember(IMemberMappingContext parentContext)
        {
            return new BasicMappingData(
                parentContext.MappingContext.RuleSet,
                parentContext.SourceType,
                parentContext.TargetMember.Type,
                QualifiedMember.None,
                parentContext);
        }

        public IMappingData Parent { get; }

        public string RuleSetName { get; }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public QualifiedMember TargetMember { get; }
    }
}