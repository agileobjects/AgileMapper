namespace AgileObjects.AgileMapper.UnitTests
{
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOnToComplexTypeMembers
    {
        [Fact]
        public void ShouldMapAMemberProperty()
        {
            var source = new Person
            {
                Address = new Address
                {
                    Line1 = "Over here!",
                    Line2 = "Yes, here!"
                }
            };

            var target = new Person
            {
                Address = new Address
                {
                    Line1 = "Over there!"
                }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.Address.Line1.ShouldNotBe("Over here!");
            result.Address.Line1.ShouldBe("Over there!");
            result.Address.Line2.ShouldBe("Yes, here!");
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            var source = new Person { Name = "Freddie" };
            var target = new Person { Address = new Address { Line1 = "Over here" } };

            var result = Mapper.Map(source).OnTo(target);

            result.Address.ShouldBe(target.Address);
        }

        [Fact]
        public void ShouldUpdateANullReadOnlyNestedMemberProperty()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = "New value" } };
            var address = new Address { Line1 = null };
            var target = new PublicReadOnlyField<Address>(address);
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeSameAs(address);
            result.Value.Line1.ShouldBe("New value");
        }

        [Fact]
        public void ShouldNotOverwriteANonNullReadOnlyNestedMemberProperty()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = "Nope" } };
            var address = new Address { Line1 = "Yep" };
            var target = new PublicReadOnlyProperty<Address>(address);
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeSameAs(address);
            result.Value.Line1.ShouldBe("Yep");
        }

        [Fact]
        public void ShouldHandleANullReadOnlyNestedMemberProperty()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = "New value" } };
            var target = new PublicReadOnlyProperty<Address>(default(Address));
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBeNull();
        }
    }
}
