namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using Api.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembersInline
    {
        [Fact]
        public void ShouldIgnoreMultipleConfiguredMembersViaExtensionMethodInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var id = Guid.NewGuid();

                var result1 = mapper
                    .Map(new CustomerViewModel
                    {
                        Id = id,
                        Name = "Bilbo",
                        Discount = 0.50,
                        AddressLine1 = "House Street"
                    })
                    .ToANew<Customer>(cfg => cfg.IgnoreNameAndDiscount());

                result1.Id.ShouldBe(id);
                result1.Name.ShouldBeNull();
                result1.Discount.ShouldBeDefault();
                result1.Address.ShouldNotBeNull();
                result1.Address.Line1.ShouldBe("House Street");

                var result2 = mapper
                    .Map(new CustomerViewModel
                    {
                        Id = id,
                        Name = "Gandalf",
                        Discount = 0.40,
                        AddressLine1 = "Maison Road"
                    })
                    .ToANew<Customer>(cfg => cfg.IgnoreNameAndDiscount());

                result2.Id.ShouldBe(id);
                result2.Name.ShouldBeNull();
                result2.Discount.ShouldBeDefault();
                result2.Address.ShouldNotBeNull();
                result2.Address.Line1.ShouldBe("Maison Road");

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberConditionallyInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var matchingResult = mapper
                    .Map(new CustomerViewModel { Name = "Bilbo" })
                    .ToANew<Customer>(cfg => cfg
                        .If(ctx => ctx.Source.Name == "Bilbo")
                        .Ignore(c => c.Name));

                matchingResult.Name.ShouldBeNull();

                var nonMatchingResult = mapper
                    .Map(new CustomerViewModel { Name = "Frodo" })
                    .ToANew<Customer>(cfg => cfg
                        .If(ctx => ctx.Source.Name == "Bilbo")
                        .Ignore(c => c.Name));

                nonMatchingResult.Name.ShouldBe("Frodo");

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }
    }

    #region Helper Classes

    internal static class ConfigurationExtensions
    {
        public static void IgnoreNameAndDiscount(
            this IFullMappingConfigurator<CustomerViewModel, Customer> config)
        {
            config.Ignore(c => c.Name, c => c.Discount);
        }
    }

    #endregion
}
