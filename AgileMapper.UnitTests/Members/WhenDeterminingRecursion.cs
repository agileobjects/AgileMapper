namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using AgileMapper.Members;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenDeterminingRecursion : MemberTestsBase
    {
        [Fact]
        public void ShouldNotCountARootMemberAsRecursive()
        {
            var rootMember = new RootQualifiedMemberFactory(MapperContext.Default)
                .RootTarget<PublicProperty<string>>();

            rootMember.IsRecursive.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotCountARootChildMemberAsRecursive()
        {
            var rootChildMember = TargetMemberFor<Parent>(p => p.EldestChild);

            rootChildMember.IsRecursive.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotCountARootChildParentMemberAsRecursive()
        {
            var rootChildParentMember = TargetMemberFor<Parent>(
                p => p.EldestChild.EldestParent);

            rootChildParentMember.IsRecursive.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotCountASimpleTypeChildMemberAsRecursive()
        {
            var rootChildParentChildMember = TargetMemberFor<PublicField<Person>>(
                p => p.Value.Address.Line1);

            rootChildParentChildMember.IsRecursive.ShouldBeFalse();
        }

        [Fact]
        public void ShouldFindRecusionInAnImmediateCircularRelationship()
        {
            var circularChildMember = TargetMemberFor<SelfReferencingClass>(c => c.Reference.Reference);

            circularChildMember.IsRecursive.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindRecusionViaAComplexTypeIntermediate()
        {
            var rootChildParentChildMember = TargetMemberFor<Parent>(
                p => p.EldestChild.EldestParent.EldestChild);

            rootChildParentChildMember.IsRecursive.ShouldBeTrue();
        }

        [Fact]
        public void ShouldFindDeeperRecusionViaAComplexTypeIntermediate()
        {
            var rootChildParentChildParentMember = TargetMemberFor<Parent>(
                p => p.EldestChild.EldestParent.EldestChild.EldestParent);

            rootChildParentChildParentMember.IsRecursive.ShouldBeTrue();
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

            rootChildEnumerableElementChildMember.IsRecursive.ShouldBeTrue();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SelfReferencingClass
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public SelfReferencingClass Reference { get; set; }
        }
    }
}
