namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMatchingSourceToTargetMembers : MemberFinderTestsBase
    {
        [Fact]
        public void ShouldMatchASameNameSameTypeFieldToAProperty()
        {
            var sourceMember = SourceMemberFor<PublicField<string>>(x => x.Value);
            var targetMember = TargetMemberFor<PublicProperty<string>>(x => x.Value);

            sourceMember.Matches(targetMember).ShouldBeTrue();
        }

        [Fact]
        public void ShouldNotMatchADifferentNameSameTypeFieldToAProperty()
        {
            var sourceMember = SourceMemberFor<PublicField<Guid>>(x => x.Value);
            var targetMember = TargetMemberFor<Person>(x => x.Id);

            sourceMember.Matches(targetMember).ShouldBeFalse();
        }

        [Fact]
        public void ShouldMatchDottedToFlatNamedSameTypeProperties()
        {
            var sourceMember = SourceMemberFor<Person>(x => x.Address.Line1);
            var targetMember = TargetMemberFor<PersonViewModel>(x => x.AddressLine1);

            sourceMember.Matches(targetMember).ShouldBeTrue();
        }

        [Fact]
        public void ShouldMatchFlatToDottedNamedSameTypeProperties()
        {
            var sourceMember = SourceMemberFor<PersonViewModel>(x => x.AddressLine1);
            var targetMember = TargetMemberFor<Person>(x => x.Address.Line1);

            sourceMember.Matches(targetMember).ShouldBeTrue();
        }

        [Fact]
        public void ShouldMatchAGetMethodToAnEquivalentNameSameTypeField()
        {
            var sourceMember = SourceMemberFor<PublicGetMethod<int>>(x => x.GetValue());
            var targetMember = TargetMemberFor<PublicField<int>>(x => x.Value);

            sourceMember.Matches(targetMember).ShouldBeTrue();
        }

        [Fact]
        public void ShouldMatchAnIdentifierPropertyToASameTypeIdProperty()
        {
            var sourceMember = SourceMemberFor(new { Identifier = Guid.NewGuid() });
            var targetMember = TargetMemberFor<Person>(x => x.Id);

            sourceMember.Matches(targetMember).ShouldBeTrue();
        }

        [Fact]
        public void ShouldMatchAnIdentifierPropertyToASameTypeTypeNameIdProperty()
        {
            var sourceMember = SourceMemberFor(new { Identifier = "123XYZ" });
            var targetMember = TargetMemberFor<Product>(x => x.ProductId);

            sourceMember.Matches(targetMember).ShouldBeTrue();
        }

        [Fact]
        public void ShouldMatchFlatIdentifierToDottedIdSameTypeProperties()
        {
            var sourceMember = SourceMemberFor(new { ValueIdentifier = Guid.NewGuid() });
            var targetMember = TargetMemberFor<PublicProperty<Customer>>(x => x.Value.Id);

            sourceMember.Matches(targetMember).ShouldBeTrue();
        }

        [Fact]
        public void ShouldMatchFlatIdToDottedTypeNameIdSameTypeProperties()
        {
            var sourceMember = SourceMemberFor(new { ValueId = "6473982" });
            var targetMember = TargetMemberFor<PublicProperty<Product>>(x => x.Value.ProductId);

            sourceMember.Matches(targetMember).ShouldBeTrue();
        }
    }
}
