namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Configuration;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingDerivedTypeMappers
    {
        [Fact]
        public void ShouldBuildADerivedTypeCreateNewMapper()
        {
            var baseTypeSource = new Product { ProductId = "111", Price = 19.99 };
            var baseTypeResult = GeneratedMapper.Map(baseTypeSource).ToANew<ProductDto>();

            baseTypeResult.ProductId.ShouldBe("111");
            baseTypeResult.Price.ShouldBe(19.99m);

            var derivedTypeSource = new MegaProduct
            {
                ProductId = "222",
                Price = 119.99,
                HowMega = 1.0m
            };

            var derivedTypeToBaseTypeResult = GeneratedMapper
                .Map(derivedTypeSource)
                .ToANew<ProductDto>()
                .ShouldBeOfType<ProductDtoMega>();

            derivedTypeToBaseTypeResult.ProductId.ShouldBe("222");
            derivedTypeToBaseTypeResult.Price.ShouldBe(119.99m);
            derivedTypeToBaseTypeResult.HowMega.ShouldBe("1.0");

            var derivedTypeToDerivedTypeResult = GeneratedMapper
                .Map(derivedTypeSource)
                .ToANew<ProductDtoMega>()
                .ShouldBeOfType<ProductDtoMega>();

            derivedTypeToDerivedTypeResult.ProductId.ShouldBe("222");
            derivedTypeToDerivedTypeResult.Price.ShouldBe(119.99m);
            derivedTypeToDerivedTypeResult.HowMega.ShouldBe("1.0");
        }

        #region Configuration

        public class DerivedTypeMapperConfiguration : BuildableMapperConfiguration
        {
            protected override void Configure()
            {
                GetPlanFor<Product>().ToANew<ProductDto>(cfg => cfg
                    .Map<MegaProduct>().To<ProductDtoMega>());
            }
        }

        #endregion
    }
}
