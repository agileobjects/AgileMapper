namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal interface IMemberMappingData
    {
        MappingRuleSet RuleSet { get; }

        IMemberMapperData MapperData { get; }

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);
    }
}