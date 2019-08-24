namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    using Members;

    internal interface IMemberFilterIgnore : IMemberIgnoreBase
    {
        string MemberFilter { get; }

        bool IsFiltered(QualifiedMember member);
    }
}