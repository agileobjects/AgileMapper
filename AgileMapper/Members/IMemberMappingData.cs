namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal interface IMemberMappingData
    {
        MappingRuleSet RuleSet { get; }

        MemberMapperData MapperData { get; }

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);
    }
}