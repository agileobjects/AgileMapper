namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    // ReSharper disable PossibleNullReferenceException
    public class WhenFindingTargetMembers : MemberTestsBase
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldFindAPublicProperty()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicProperty<byte>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(byte));
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldFindAPublicField()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicField<int>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(int));
            member.IsWriteable.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldFindAPublicReadOnlyField()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicReadOnlyField<IEnumerable<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(IEnumerable<byte>));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldFindAPublicSetMethod()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicSetMethod<DateTime>))
                .FirstOrDefault(m => m.Name == "SetValue");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(DateTime));
            member.IsWriteable.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldFindAPublicReadOnlyComplexTypeProperty()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicReadOnlyProperty<object>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(object));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
        public void ShouldFindAPublicReadOnlySimpleTypeProperty()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicReadOnlyProperty<long>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(long));
            member.IsWriteable.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
        public void ShouldIgnoreANonPublicField()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(InternalField<List<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldIgnoreAGetMethod()
        {
            var member = MemberCache
                .GetTargetMembers(typeof(PublicGetMethod<string[]>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
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
