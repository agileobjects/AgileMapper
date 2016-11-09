namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingDerivedTypes
    {
        [Fact]
        public void ShouldMapARootComplexTypeFromItsAssignedType()
        {
            object source = new Product { Price = 100.00 };
            var result = Mapper.Map(source).ToANew<Product>();

            result.Price.ShouldBe(100.00);
        }

        [Fact]
        public void ShouldMapARootComplexTypeEnumerableFromItsAssignedType()
        {
            object source = new[] { new Product { Price = 10.01 } };
            var result = Mapper.Map(source).ToANew<IEnumerable<Product>>();

            result.First().Price.ShouldBe(10.01);
        }

        [Fact]
        public void ShouldMapARootComplexTypeEnumerableElementFromItsAssignedType()
        {
            var source = new object[] { new Product { Price = 9.99 } };
            var result = Mapper.Map(source).ToANew<IEnumerable<Product>>();

            result.First().Price.ShouldBe(9.99);
        }

        [Fact]
        public void ShouldMapAComplexTypeMemberFromItsAssignedType()
        {
            var source = new PublicProperty<object>
            {
                Value = new { Name = "Frank", Address = (object)new Address { Line1 = "Here!" } }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<PersonViewModel>>();

            result.Value.ShouldNotBeNull();
            result.Value.Name.ShouldBe("Frank");
            result.Value.AddressLine1.ShouldBe("Here!");
        }

        [Fact]
        public void ShouldMapAComplexTypeMemberInACollectionFromItsAssignedType()
        {
            var sourceObjectId = Guid.NewGuid();

            var source = new object[]
            {
                new { Name = "Bob", Address = new { Line1 = "There!" } },
                new { Id = sourceObjectId.ToString(), Address = (object)new Address { Line1 = "Somewhere!" } }
            };

            var result = Mapper.Map(source).ToANew<ICollection<PersonViewModel>>();

            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);

            result.First().Id.ShouldBeDefault();
            result.First().Name.ShouldBe("Bob");
            result.First().AddressLine1.ShouldBe("There!");

            result.Second().Id.ShouldBe(sourceObjectId);
            result.Second().Name.ShouldBeNull();
            result.Second().AddressLine1.ShouldBe("Somewhere!");
        }

        [Fact]
        public void ShouldMapAComplexTypeEnumerableMemberFromItsSourceType()
        {
            var source = new PublicField<object>
            {
                Value = new Collection<object>
                {
                    new Customer { Name = "Fred" },
                    new Person { Name = "Bob" }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<object>>();

            var resultValues = ((IEnumerable)result.Value).Cast<Person>().ToArray();
            resultValues.First().ShouldBeOfType<Customer>();
            resultValues.Second().ShouldBeOfType<Person>();
        }

        [Fact]
        public void ShouldMapDerivedTypesFromNestedMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Map<CustomerViewModel>()
                    .To<Customer>();

                var personSource = new PublicField<CustomerViewModel>
                {
                    Value = new MysteryCustomerViewModel { Discount = 0.2 }
                };
                var result = mapper.Map(personSource).ToANew<PublicSetMethod<Person>>();

                result.Value.ShouldBeOfType<Customer>();
                ((Customer)result.Value).Discount.ShouldBe(0.2);
            }
        }

        [Fact]
        public void ShouldConditionallyMapDerivedTypesFromNestedMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<CustomerViewModel>()
                    .If((s, t) => s.Name == "Mystery Customer")
                    .MapTo<MysteryCustomerViewModel>()
                    .And
                    .If((s, t) => s.Name == "Customer Mystery!")
                    .MapTo<MysteryCustomerViewModel>();

                var personSource = new PublicField<PersonViewModel>
                {
                    Value = new CustomerViewModel { Name = "Mystery Customer", Discount = 0.5 }
                };
                var result = mapper.Map(personSource).ToANew<PublicProperty<PersonViewModel>>();

                result.Value.ShouldBeOfType<MysteryCustomerViewModel>();
                ((MysteryCustomerViewModel)result.Value).Discount.ShouldBe(0.5);
            }
        }
    }
}
