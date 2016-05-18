namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Members;
    using Caching;
    using TestClasses;
    using Xunit;

    public class WhenCreatingTargetMembersFromExpressions
    {
        private static readonly MemberFinder _memberFinder = new MemberFinder(new DictionaryCache());

        [Fact]
        public void ShouldCreateAFieldMember()
        {
            Expression<Func<PublicField<string>, string>> fieldAccess = x => x.Value;

            var fieldMember = fieldAccess.ToTargetMember(_memberFinder);

            fieldMember.Members().Count().ShouldBe(2);

            fieldMember.Members().Last().Name.ShouldBe("Value");
            fieldMember.Members().Last().Type.ShouldBe(typeof(string));

            fieldMember.Members().First().Type.ShouldBe(typeof(PublicField<string>));
        }

        [Fact]
        public void ShouldCreateANestedPropertyMember()
        {
            Expression<Func<Person, string>> addressLine1Access = x => x.Address.Line1;

            var addressLine1Member = addressLine1Access.Body.ToTargetMember(_memberFinder);

            addressLine1Member.Members().Count().ShouldBe(3);

            addressLine1Member.Members().Last().Name.ShouldBe("Line1");
            addressLine1Member.Members().Last().Type.ShouldBe(typeof(string));

            addressLine1Member.Members().Second().Name.ShouldBe("Address");
            addressLine1Member.Members().Second().Type.ShouldBe(typeof(Address));

            addressLine1Member.Members().First().Type.ShouldBe(typeof(Person));
        }

        [Fact]
        public void ShouldCreateASetMethodMember()
        {
            Expression<Func<PublicSetMethod<int>, Action<int>>> setMethodAccess = x => x.SetValue;

            var fieldMember = setMethodAccess.Body.ToTargetMember(_memberFinder);

            fieldMember.Members().Count().ShouldBe(2);

            fieldMember.Members().Last().Name.ShouldBe("SetValue");
            fieldMember.Members().Last().Type.ShouldBe(typeof(int));

            fieldMember.Members().First().Type.ShouldBe(typeof(PublicSetMethod<int>));
        }
    }

    internal static class TestMemberExtensions
    {
        public static IEnumerable<Member> Members(this IQualifiedMember qualifiedMember)
        {
            return ((QualifiedMember)qualifiedMember).Members;
        }
    }
}
