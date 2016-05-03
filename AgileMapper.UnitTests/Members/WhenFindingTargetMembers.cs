namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Members;
    using Caching;
    using Shouldly;
    using TestClasses;
    using Xunit;

    // ReSharper disable PossibleNullReferenceException
    public class WhenFindingTargetMembers
    {
        private static readonly MemberFinder _memberFinder = new MemberFinder(new DictionaryCache());

        [Fact]
        public void ShouldFindAPublicProperty()
        {
            var member = _memberFinder
                .GetTargetMembers(typeof(PublicProperty<byte>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(byte));
        }

        [Fact]
        public void ShouldFindAPublicField()
        {
            var member = _memberFinder
                .GetTargetMembers(typeof(PublicField<int>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(int));
        }

        [Fact]
        public void ShouldFindAPublicSetMethod()
        {
            var member = _memberFinder
                .GetTargetMembers(typeof(PublicSetMethod<DateTime>))
                .FirstOrDefault(m => m.Name == "SetValue");

            member.ShouldNotBeNull();
            member.Type.ShouldBe(typeof(DateTime));
        }

        [Fact]
        public void ShouldIgnoreAReadOnlyPublicProperty()
        {
            var member = _memberFinder
                .GetTargetMembers(typeof(PublicReadOnlyProperty<long>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreANonPublicField()
        {
            var member = _memberFinder
                .GetTargetMembers(typeof(InternalField<List<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAReadOnlyField()
        {
            var member = _memberFinder
                .GetTargetMembers(typeof(ReadOnlyField<IEnumerable<byte>>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAGetMethod()
        {
            var member = _memberFinder
                .GetTargetMembers(typeof(PublicGetMethod<string[]>))
                .FirstOrDefault(m => m.Name == "Value");

            member.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAPropertySetter()
        {
            var member = _memberFinder
                .GetTargetMembers(typeof(PublicProperty<long>))
                .FirstOrDefault(m => m.Name.StartsWith("set_"));

            member.ShouldBeNull();
        }
    }
    // ReSharper restore PossibleNullReferenceException
}
