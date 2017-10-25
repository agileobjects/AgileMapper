namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
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
    }
}
