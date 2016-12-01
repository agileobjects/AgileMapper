namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    // ReSharper disable PossibleNullReferenceException
    public class WhenFindingTargetMembers : MemberTestsBase
    {
        [Fact]
        public void ShouldFindAPublicProperty()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicProperty<byte>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(byte));
        }

        [Fact]
        public void ShouldFindAPublicField()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicField<int>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(int));
            member.IsWriteable.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindAPublicReadOnlyField()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicReadOnlyField<IEnumerable<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(IEnumerable<byte>));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldFindAPublicSetMethod()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicSetMethod<DateTime>))
                .FirstOrDefault(m => m.Name == "SetValue");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(DateTime));
            member.IsWriteable.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindAPublicReadOnlyComplexTypeProperty()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicReadOnlyProperty<object>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(object));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldIgnoreANonPublicField()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(InternalField<List<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAPublicReadOnlyArrayField()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicReadOnlyField<byte[]>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAPublicReadOnlySimpleTypeProperty()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicReadOnlyProperty<long>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAReadOnlyArrayProperty()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicReadOnlyProperty<long[]>))
                .FirstOrDefault(m => m.Name.StartsWith("Value"));

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAGetMethod()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicGetMethod<string[]>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAPropertySetter()
        {
            var member = MemberFinder
                .GetTargetMembers(typeof(PublicProperty<long>))
                .FirstOrDefault(m => m.Name.StartsWith("set_"));

            member.ShouldBeNull();
        }
    }
    // ReSharper restore PossibleNullReferenceException
}
