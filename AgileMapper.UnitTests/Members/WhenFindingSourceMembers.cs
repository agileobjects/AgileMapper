namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    // ReSharper disable PossibleNullReferenceException
    public class WhenFindingSourceMembers : MemberTestsBase
    {
        [Fact]
        public void ShouldFindAPublicProperty()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(PublicProperty<string>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(string));
        }

        [Fact]
        public void ShouldFindAPublicField()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(PublicField<int>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(int));
        }

        [Fact]
        public void ShouldFindAPublicGetMethod()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(PublicGetMethod<DateTime>))
                .FirstOrDefault(m => m.Name == "GetValue");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(DateTime));
        }

        [Fact]
        public void ShouldFindARootArrayElement()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(int[]))
                .FirstOrDefault();

            member.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldIgnoreAWriteOnlyPublicProperty()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(PublicWriteOnlyProperty<long>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreANonPublicField()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(InternalField<byte>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreASetMethod()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(PublicSetMethod<short>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreGetType()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(PublicProperty<int?>))
                .FirstOrDefault(m => m.Name == "GetType");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreGetHashCode()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(PublicProperty<DateTime?>))
                .FirstOrDefault(m => m.Name == "GetHashCode");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAPropertyGetter()
        {
            var member = MemberFinder
                .GetSourceMembers(typeof(PublicProperty<string>))
                .FirstOrDefault(m => m.Name.StartsWith("get_"));

            member.ShouldBeNull();
        }
    }
    // ReSharper restore PossibleNullReferenceException
}
