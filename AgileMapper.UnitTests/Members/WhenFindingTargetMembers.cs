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
            var member = MemberCache
                .GetTargetMembers(typeof(PublicProperty<byte>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(byte));
        }

        [Fact]
        public void ShouldFindAPublicField()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicField<int>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(int));
            member.IsWriteable.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindAPublicReadOnlyField()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicReadOnlyField<IEnumerable<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(IEnumerable<byte>));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldFindAPublicSetMethod()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicSetMethod<DateTime>))
                .FirstOrDefault(m => m.Name == "SetValue");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(DateTime));
            member.IsWriteable.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindAPublicReadOnlyComplexTypeProperty()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicReadOnlyProperty<object>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(object));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldFindAPublicReadOnlyArrayField()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicReadOnlyField<byte[]>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(byte[]));
            member.ElementType.ShouldBe(typeof(byte));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldFindAPublicReadOnlySimpleTypeProperty()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicReadOnlyProperty<long>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(long));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldFindAReadOnlyArrayProperty()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicReadOnlyProperty<long[]>))
                .FirstOrDefault(m => m.Name.StartsWith("Value"));

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(long[]));
            member.ElementType.ShouldBe(typeof(long));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact]
        public void ShouldIgnoreANonPublicField()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(InternalField<List<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAGetMethod()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicGetMethod<string[]>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAPropertySetter()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicProperty<long>))
                .FirstOrDefault(m => m.Name.StartsWith("set_"));

            member.ShouldBeNull();
        }
    }
    // ReSharper restore PossibleNullReferenceException
}
