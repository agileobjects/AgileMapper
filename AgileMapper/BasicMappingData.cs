namespace AgileObjects.AgileMapper
{
    using System;
    using Members;

    internal class BasicMappingData : IMappingData
    {
        public BasicMappingData(
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
            RuleSetName = ruleSet.Name;
        }

        public IMappingData Parent => null;

        public string RuleSetName { get; }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public QualifiedMember TargetMember => QualifiedMember.All;
    }
}