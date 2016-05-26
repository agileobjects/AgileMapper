namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    // ReSharper disable PossibleNullReferenceException
    public class WhenFindingTargetMembers : MemberFinderTestsBase
    {
        [Fact]
        public void ShouldFindAPublicProperty()
        {
            var member = MemberFinder
                .GetWriteableMembers(typeof(PublicProperty<byte>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(byte));
        }

        [Fact]
        public void ShouldFindAPublicField()
        {
            var member = MemberFinder
                .GetWriteableMembers(typeof(PublicField<int>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(int));
        }

        [Fact]
        public void ShouldFindAPublicSetMethod()
        {
            var member = MemberFinder
                .GetWriteableMembers(typeof(PublicSetMethod<DateTime>))
                .FirstOrDefault(m => m.Name == "SetValue");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(DateTime));
        }

        [Fact]
        public void ShouldIgnoreAReadOnlyPublicProperty()
        {
            var member = MemberFinder
                .GetWriteableMembers(typeof(PublicReadOnlyProperty<long>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreANonPublicField()
        {
            var member = MemberFinder
                .GetWriteableMembers(typeof(InternalField<List<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAReadOnlyField()
        {
            var member = MemberFinder
                .GetWriteableMembers(typeof(ReadOnlyField<IEnumerable<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAGetMethod()
        {
            var member = MemberFinder
                .GetWriteableMembers(typeof(PublicGetMethod<string[]>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAPropertySetter()
        {
            var member = MemberFinder
                .GetWriteableMembers(typeof(PublicProperty<long>))
                .FirstOrDefault(m => m.Name.StartsWith("set_"));

            member.ShouldBeNull();
        }
    }
    // ReSharper restore PossibleNullReferenceException
}
