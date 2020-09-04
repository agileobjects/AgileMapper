namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using AgileMapper.Members;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenDeterminingRecursion : MemberTestsBase
    {
        [Fact]
        public void ShouldNotCountARootMemberAsRecursive()
        {
            var rootMember = new QualifiedMemberFactory(DefaultMapperContext)
                .RootTarget<Person, PublicProperty<string>>();

            rootMember.IsRecursion.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotCountARootChildMemberAsRecursive()
        {
            var rootChildMember = TargetMemberFor<Parent>(p => p.EldestChild);

            rootChildMember.IsRecursion.ShouldBeFalse();
        }

        [Fact]
        public void ShouldCountARootChildParentMemberAsRecursive()
        {
            var rootChildParentMember = TargetMemberFor<Parent>(
                p => p.EldestChild.EldestParent);

            rootChildParentMember.IsRecursion.ShouldBeTrue();
        }

        [Fact]
        public void ShouldNotCountASimpleTypeChildMemberAsRecursive()
        {
            var rootChildParentChildMember = TargetMemberFor<PublicField<Person>>(
                p => p.Value.Address.Line1);

            rootChildParentChildMember.IsRecursion.ShouldBeFalse();
        }

        [Fact]
        public void ShouldFindRecusionInAnImmediateCircularRelationship()
        {
            var circularChildMember = TargetMemberFor<SelfReferencingClass>(c => c.Reference.Reference);

            circularChildMember.IsRecursion.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindRecusionViaAComplexTypeIntermediate()
        {
            var rootChildParentChildMember = TargetMemberFor<Parent>(
                p => p.EldestChild.EldestParent.EldestChild);

            rootChildParentChildMember.IsRecursion.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindDeeperRecusionViaAComplexTypeIntermediate()
        {
            var rootChildParentChildParentMember = TargetMemberFor<Parent>(
                p => p.EldestChild.EldestParent.EldestChild.EldestParent);

            rootChildParentChildParentMember.IsRecursion.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindRecusionViaAnEnumerableIntermediate()
        {
            var rootChildEnumerableMember = TargetMemberFor<FacebookUser>(u => u.Friends);

            // FacebookUser.Friends[i]
            var rootChildEnumerableElementMember = rootChildEnumerableMember.GetElementMember();

            // FacebookUser.Friends[i].Friends
            var rootChildEnumerableElementChildMember = rootChildEnumerableElementMember
                .Append(rootChildEnumerableMember.LeafMember);

            rootChildEnumerableElementChildMember.IsRecursion.ShouldBeTrue();
        }

        #region Helper Class

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SelfReferencingClass
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public SelfReferencingClass Reference { get; set; }
        }

        #endregion
    }
}
