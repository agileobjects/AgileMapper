namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using Api.Configuration;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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

        [Fact]
        public void ShouldExtendMemberIgnoreConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, int>>()
                    .To<PublicTwoFields<long, long>>()
                    .If((sptf, tptf) => sptf.Value1 < 5)
                    .Ignore(ptf => ptf.Value1);      // Ignore target.Value1 if source.Value1 < 5

                var result1 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 4, Value2 = 8 })
                    .OnTo(new PublicTwoFields<long, long>(), c => c
                        .If((sptf, tptf) => sptf.Value2 <= 10)
                        .Ignore(ptf => ptf.Value2)); // Ignore target.Value2 if source.Value2 <= 10

                result1.Value1.ShouldBeDefault();
                result1.Value2.ShouldBeDefault();

                var result2 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 5, Value2 = 7 })
                    .OnTo(new PublicTwoFields<long, long>(), c => c
                        .If((sptf, tptf) => sptf.Value2 <= 10)
                        .Ignore(ptf => ptf.Value2)); // Ignore target.Value2 if source.Value2 <= 10

                result2.Value1.ShouldBe(5);
                result2.Value2.ShouldBeDefault();

                mapper.InlineContexts().ShouldHaveSingleItem();

                var result3 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 5, Value2 = 11 })
                    .OnTo(new PublicTwoFields<long, long>(), c => c
                        .If((sptf, tptf) => sptf.Value1 >= 3)
                        .Ignore(ptf => ptf.Value1) // Ignore target.Value1 if source.Value1 >= 3
                        .And
                        .If((sptf, tptf) => sptf.Value2 <= 10)
                        .Ignore(ptf => ptf.Value2)); // Ignore target.Value2 if source.Value2 < 10

                result3.Value1.ShouldBeDefault();
                result3.Value2.ShouldBe(11);

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldIgnoreMembersUsingAnInlineMemberTypeFilter()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new ProductDto { ProductId = "Inline", Price = 10.00m })
                    .Over(new Product { ProductId = "Outline", Price = 9.99 }, cfg => cfg
                        .IgnoreTargetMembersOfType<double>());

                result.ProductId.ShouldBe("Inline");
                result.Price.ShouldBe(9.99);
            }
        }

        [Fact]
        public void ShouldIgnoreMembersUsingAnInlinePropertyMatcherFilter()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(new ProductDto { ProductId = "Inline", Price = 10.00m })
                    .Over(new Product { ProductId = "Outline", Price = 9.99 }, cfg => cfg
                        .IgnoreTargetMembersWhere(m => m.IsPropertyMatching(pi => pi.Name.Length < 6)));

                result1.ProductId.ShouldBe("Inline");
                result1.Price.ShouldBe(9.99);

                var result2 = mapper
                    .Map(new ProductDto { ProductId = "Inline", Price = 1.00m })
                    .Over(new Product { ProductId = "Outline" }, cfg => cfg
                        .IgnoreTargetMembersWhere(m => m.IsPropertyMatching(pi => pi.Name.Length < 6)));

                result2.ProductId.ShouldBe("Inline");
                result2.Price.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldExtendMemberFilterConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<ProductDto>()
                    .To<Product>()
                    .IgnoreTargetMembersWhere(m => m.Name.Length <= 5);

                var result1 = mapper
                    .Map(new ProductDto { ProductId = "Inline", Price = 10.00m })
                    .ToANew<Product>(cfg => cfg
                        .IgnoreTargetMembersWhere(m => m.Name.Length - 1 >= 8));

                result1.ProductId.ShouldBeDefault();
                result1.Price.ShouldBeDefault();

                var result2 = mapper
                    .Map(new ProductDto { ProductId = "Inline", Price = 1.00m })
                    .ToANew<Product>(cfg => cfg
                        .IgnoreTargetMembersWhere(m => m.Name.Length - 1 >= 8));

                result2.ProductId.ShouldBeDefault();
                result2.Price.ShouldBeDefault();
            }
        }
    }

    #region Helper Classes

    internal static class ConfigurationExtensions
    {
        public static void IgnoreNameAndDiscount(
            this IFullMappingInlineConfigurator<CustomerViewModel, Customer> config)
        {
            config.Ignore(c => c.Name, c => c.Discount);
        }
    }

    #endregion
}
