namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    internal interface IMemberFilterIgnore : IMemberIgnoreBase
    {
        string MemberFilter { get; }
    }
}