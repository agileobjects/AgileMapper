namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal interface IMemberMapperCreationData : IMapperCreationData
    {
        MemberMapperData MapperData { get; }

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);

        MappingInstanceData<TSource, TTarget> CreateChildMappingInstanceData<TSource, TTarget>(IQualifiedMember sourceMember);
    }
}