namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    using Members;

    internal interface IMemberIgnore : IMemberIgnoreBase
    {
        QualifiedMember Member { get; }
    }
}