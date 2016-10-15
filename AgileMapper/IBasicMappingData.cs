namespace AgileObjects.AgileMapper
{
    using System;
    using Members;

    internal interface IBasicMappingData : IMappingData
    {
        MappingRuleSet RuleSet { get; }

        Type SourceType { get; }

        Type TargetType { get; }

        IBasicMapperData MapperData { get; }
    }
}