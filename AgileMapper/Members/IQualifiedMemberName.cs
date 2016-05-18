namespace AgileObjects.AgileMapper.Members
{
    internal interface IQualifiedMemberName
    {
        bool Matches(IQualifiedMemberName otherQualifiedName);
    }
}