namespace AgileObjects.AgileMapper.UnitTests
{
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOverComplexTypeMembers
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

            var result = Mapper.Map(source).Over(target);

            result.Address.Line1.ShouldBe(source.Address.Line1);
            result.Address.Line2.ShouldBe(source.Address.Line2);
        }

        [Fact]
        public void ShouldOverwriteAMemberToNull()
        {
            var source = new Person { Name = "Dylan" };
            var target = new Person { Address = new Address { Line1 = "Over here" } };

            var result = Mapper.Map(source).Over(target);

            result.Address.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { Hello = "You" };
            var target = new Customer { Address = new Address() };
            var result = Mapper.Map(source).Over(target);

            result.Address.ShouldNotBeNull();
        }

        // See https://github.com/agileobjects/AgileMapper/issues/21
        [Fact]
        public void ShouldNotPopulateAMemberWithNoMatchingSource()
        {
            var source = new { Name = "Customer!" };
            var target = new Customer { Name = "No-one", Address = default(Address) };
            var result = Mapper.Map(source).Over(target);

            result.Name.ShouldBe("Customer!");
            result.Address.ShouldBeNull();
        }

        // See https://github.com/agileobjects/AgileMapper/issues/34
        [Fact]
        public void ShouldNotPopulateAMemberWithANullSource()
        {
            var result = Mapper.Map(new RockModel()).Over(new PaperModel());

            result.Paper.ShouldBeNull();
        }

        [Fact]
        public void ShouldNotOverwriteAMemberWithNoMatchingSource()
        {
            var source = new { Name = "Scooby" };
            var target = new MysteryCustomer
            {
                Name = "No-one",
                Address = new Address { Line1 = "Leave me alone!" }
            };
            var originalAddress = target.Address;
            var result = Mapper.Map(source).Over(target);

            result.Name.ShouldBe("Scooby");
            result.Address.ShouldBeSameAs(originalAddress);
        }

        [Fact]
        public void ShouldHandleANullConfiguredSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .Over<Person>()
                    .Map(ctx => ctx.Source.Address.Line1)
                    .To(x => x.Address.Line2);

                var source = new Person { Name = "Scott" };
                var target = new Person { Address = new Address() };
                var result = mapper.Map(source).Over(target);

                result.Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldOverwriteANonNullReadOnlyNestedMemberProperty()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = "New value" } };
            var address = new Address { Line1 = "Original value" };
            var target = new PublicReadOnlyField<Address>(address);
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeSameAs(address);
            result.Value.Line1.ShouldBe("New value");
        }

        [Fact]
        public void ShouldOverwriteANonNullReadOnlyNestedMemberPropertyToNull()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = null } };
            var address = new Address { Line1 = "Didn't start as null" };
            var target = new PublicReadOnlyField<Address>(address);
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeSameAs(address);
            result.Value.Line1.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleANullReadOnlyNestedMemberProperty()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = "New value" } };
            var target = new PublicReadOnlyProperty<Address>(default(Address));
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }

        #region Helper Classes

        internal class PaperModel
        {
            public int Id { get; set; }

            public int? PaperId { get; set; }

            public Paper Paper { get; set; }
        }

        internal class RockModel
        {
            public int Id { get; set; }

            public int? RockId { get; set; }

            public Rock Rock { get; set; }
        }

        internal class Rock
        {
            public int Id { get; set; }
        }

        internal class Paper
        {
            public int Id { get; set; }
        }

        #endregion
    }
}