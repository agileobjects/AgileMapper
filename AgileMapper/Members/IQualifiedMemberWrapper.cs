namespace AgileObjects.AgileMapper.Members
{
    internal interface IQualifiedMemberWrapper : IQualifiedMember
    {
        IQualifiedMember WrappedMember { get; }
    }
}