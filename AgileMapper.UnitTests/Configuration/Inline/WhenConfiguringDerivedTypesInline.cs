namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDerivedTypesInline
    {
        [Fact]
        public void ShouldMapACustomTypePairInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Product product = new MegaProduct
                {
                    ProductId = "Pretty-Darn-Mega",
                    Price = 99.99,
                    HowMega = 0.99m
                };

                var result = mapper
                    .Map(product)
                    .ToANew<ProductDto>(cfg => cfg
                        .Map<MegaProduct>()
                        .To<ProductDtoMega>());

                result.ShouldBeOfType<ProductDtoMega>();
                result.ProductId.ShouldBe("Pretty-Darn-Mega");
                result.Price.ShouldBe(99.99m);
                ((ProductDtoMega)result).HowMega.ShouldBe("0.99");
            }
        }

        [Fact]
        public void ShouldMapACustomTypePairInACollectionInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var products = new[]
                {
                    new Product { ProductId = "Pretty-Darn", Price = 9.99 },
                    new MegaProduct { ProductId = "Pretty-Darn-Mega", Price = 99.99, HowMega = 0.99m }
                };

                var result1 = mapper
                    .Map(products)
                    .ToANew<ProductDto[]>(cfg => cfg
                        .WhenMapping
                        .From<Product>()
                        .To<ProductDto>()
                        .Map<MegaProduct>()
                        .To<ProductDtoMega>());

                result1.Length.ShouldBe(2);

                result1.First().ShouldBeOfType<ProductDto>();
                result1.First().ProductId.ShouldBe("Pretty-Darn");
                result1.First().Price.ShouldBe(9.99m);

                result1.Second().ShouldBeOfType<ProductDtoMega>();
                result1.Second().ProductId.ShouldBe("Pretty-Darn-Mega");
                result1.Second().Price.ShouldBe(99.99m);
                ((ProductDtoMega)result1.Second()).HowMega.ShouldBe("0.99");

                var result2 = mapper
                    .Map(products)
                    .ToANew<ProductDto[]>(cfg => cfg
                        .WhenMapping
                        .From<Product>()
                        .To<ProductDto>()
                        .Map<MegaProduct>()
                        .To<ProductDtoMega>());

                result2.Length.ShouldBe(2);

                result2.First().ShouldBeOfType<ProductDto>();
                result2.First().ProductId.ShouldBe("Pretty-Darn");
                result2.First().Price.ShouldBe(9.99m);

                result2.Second().ShouldBeOfType<ProductDtoMega>();
                result2.Second().ProductId.ShouldBe("Pretty-Darn-Mega");
                result2.Second().Price.ShouldBe(99.99m);
                ((ProductDtoMega)result2.Second()).HowMega.ShouldBe("0.99");

                var result3 = mapper
                    .Map(products)
                    .ToANew<ProductDto[]>(cfg => cfg
                        .WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .Map<MysteryCustomer>()
                        .To<CustomerViewModel>());

                result3.Length.ShouldBe(2);

                result3.First().ShouldBeOfType<ProductDto>();
                result3.First().ProductId.ShouldBe("Pretty-Darn");
                result3.First().Price.ShouldBe(9.99m);

                result3.Second().ShouldBeOfType<ProductDto>();
                result3.Second().ProductId.ShouldBe("Pretty-Darn-Mega");
                result3.Second().Price.ShouldBe(99.99m);

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }
    }
}
