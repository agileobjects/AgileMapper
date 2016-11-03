namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal interface IBasicMappingData : IMappingData
    {
        MappingRuleSet RuleSet { get; }

        Type SourceType { get; }

        Type TargetType { get; }
    }
}