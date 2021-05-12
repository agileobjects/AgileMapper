namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingDerivedTypeMappers
    {
        [Fact]
        public void ShouldBuildADerivedTypeCreateNewMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .Map<MegaProduct>()
                    .To<ProductDtoMega>();

                mapper.GetPlanFor<Product>().ToANew<ProductDto>();

                var sourceCodeExpressions = mapper.BuildSourceCode();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var baseTypeSource = new Product { ProductId = "111", Price = 19.99 };

                var baseTypeExecutor = staticMapMethod
                    .ShouldCreateMappingExecutor(baseTypeSource);

                var baseTypeResult = baseTypeExecutor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<ProductDto>(baseTypeExecutor);

                baseTypeResult.ProductId.ShouldBe("111");
                baseTypeResult.Price.ShouldBe(19.99m);

                var derivedTypeSource = new MegaProduct
                {
                    ProductId = "222",
                    Price = 119.99,
                    HowMega = 1.0m
                };

                var derivedTypeExecutor = staticMapMethod
                    .ShouldCreateMappingExecutor<Product>(derivedTypeSource);

                var derivedTypeBaseTypeResult = derivedTypeExecutor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<ProductDto>(derivedTypeExecutor)
                    .ShouldBeOfType<ProductDtoMega>();

                derivedTypeBaseTypeResult.ProductId.ShouldBe("222");
                derivedTypeBaseTypeResult.Price.ShouldBe(119.99m);
                derivedTypeBaseTypeResult.HowMega.ShouldBe("1.0");

                var derivedTypeDerivedTypeResult = derivedTypeExecutor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<ProductDtoMega>(derivedTypeExecutor);

                derivedTypeDerivedTypeResult.ProductId.ShouldBe("222");
                derivedTypeDerivedTypeResult.Price.ShouldBe(119.99m);
                derivedTypeDerivedTypeResult.HowMega.ShouldBe("1.0");
            }
        }
    }
}
