namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System.Linq.Expressions;
    using AgileMapper.Members;

    internal static class MemberFinderExtensions
    {
        public static QualifiedMember ToSourceMember(this Expression memberAccessExpression, MemberFinder memberFinder)
        {
            return MemberExtensions
                .CreateMember(memberAccessExpression, Member.RootSource, memberFinder.GetSourceMembers);
        }


    }
}