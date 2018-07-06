namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Configuration;
    using AgileMapper.Extensions.Internal;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingToNull
    {
        [Fact]
        public void ShouldApplyAUserConfiguredCondition()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Address>()
                    .If((o, a) => a.Line1.IsNullOrWhiteSpace())
                    .MapToNull();

                var source = new CustomerViewModel { Name = "Bob" };
                var result = mapper.Map(source).ToANew<Customer>();

                result.Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldRestrictAConfiguredMapToNullConditionBySourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<Address>()
                    .If((o, a) => a.Line1 == "Null!")
                    .MapToNull();

                var nonMatchingSource = new Customer
                {
                    Name = "Jen",
                    Address = new Address { Line1 = "Null!" }
                };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Customer>();

                nonMatchingResult.Address.ShouldNotBeNull();
                nonMatchingResult.Address.Line1.ShouldBe("Null!");

                var matchingSource = new CustomerViewModel
                {
                    Name = "Frank",
                    AddressLine1 = "Null!"
                };
                var matchingResult = mapper.Map(matchingSource).ToANew<Customer>();

                matchingResult.Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldOverwriteAPropertyToNull()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Over<Address>()
                    .If(ctx => ctx.Target.Line1 == null)
                    .MapToNull();

                var nonMatchingSource = new CustomerViewModel
                {
                    Id = Guid.NewGuid(),
                    AddressLine1 = "Places!"
                };
                var target = new Customer { Address = new Address { Line1 = "Home!" } };

                mapper.Map(nonMatchingSource).Over(target);

                target.Address.ShouldNotBeNull();
                target.Address.Line1.ShouldBe("Places!");

                var matchingSource = new CustomerViewModel { Id = Guid.NewGuid() };

                mapper.Map(matchingSource).Over(target);

                target.Address.ShouldBeNull();

                var nullAddressTarget = new Customer();

                mapper.Map(matchingSource).Over(nullAddressTarget);

                target.Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyConfiguredConditionsToDerivedTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Product>()
                    .If((o, p) => p.Price.Equals(0))
                    .MapToNull();

                mapper.WhenMapping
                    .To<MegaProduct>()
                    .If((o, p) => p.HowMega == 0)
                    .MapToNull();

                var nonMatchingProductSource = new { Price = 123 };
                var nonMatchingProductResult = mapper.Map(nonMatchingProductSource).ToANew<Product>();

                nonMatchingProductResult.Price.ShouldBe(123);

                var matchingProductSource = new { Price = 0 };
                var matchingProductResult = mapper.Map(matchingProductSource).ToANew<Product>();

                matchingProductResult.ShouldBeNull();

                var nonMatchingMegaProductSource = new { HowMega = 0.99 };
                var nonMatchingMegaProductResult = mapper.Map(nonMatchingMegaProductSource).ToANew<MegaProduct>();

                nonMatchingMegaProductResult.HowMega.ShouldBe(0.99);

                var matchingMegaProductSource = new { HowMega = 0.00 };
                var matchingMegaProductResult = mapper.Map(matchingMegaProductSource).ToANew<MegaProduct>();

                matchingMegaProductResult.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldNotMapCollectionElementsToNull()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Address>()
                    .If((o, a) => a.Line2 == "Delete me")
                    .MapToNull();

                var source = new[]
                {
                    new Address { Line1 = "Delete me" },
                    new Address { Line2 = "Delete me" }
                };
                var result = mapper.Map(source).ToANew<ICollection<Address>>();

                result.First().ShouldNotBeNull();
                result.First().Line1.ShouldBe("Delete me");

                result.Second().ShouldNotBeNull();
                result.Second().Line2.ShouldBe("Delete me");
            }
        }

        [Fact]
        public void ShouldMapCollectionElementNestedPropertiesToNull()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Address>()
                    .If((o, a) => a.Line1 == null)
                    .MapToNull();

                var source = new[]
                {
                    new CustomerViewModel { AddressLine1 = null },
                    new CustomerViewModel { AddressLine1 = "Hello!" }
                };
                var result = mapper.Map(source).ToANew<IEnumerable<MysteryCustomer>>();

                result.First().Address.ShouldBeNull();
                result.Second().Address.ShouldNotBeNull();
                result.Second().Address.Line1.ShouldBe("Hello!");
            }
        }

        [Fact]
        public void ShouldErrorIfConditionsAreConfiguredForTheSameType()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .To<Address>()
                        .If((o, a) => a.Line1 == null)
                        .MapToNull();

                    mapper.WhenMapping
                        .To<Address>()
                        .If((o, a) => a.Line1 == string.Empty)
                        .MapToNull();
                }
            });

            configEx.Message.ShouldContain("already has");
        }
    }
}
