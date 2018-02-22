namespace AgileObjects.AgileMapper.Members
{
    using System;
    using ObjectPopulation;

    internal interface IChildMemberMappingData
    {
        MappingRuleSet RuleSet { get; }

        IObjectMappingData Parent { get; }

        IMemberMapperData MapperData { get; }

        bool IsRepeatMapping(Type sourceType);

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);
    }
}