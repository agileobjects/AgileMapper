namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal interface IMemberMappingContextData : IMappingContextData
    {
        MemberMapperData MapperData { get; }

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);
    }
}