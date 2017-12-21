namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using AgileMapper.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringObjectCreationInline
    {
        [Fact]
        public void ShouldUseAConfiguredTargetInstanceFactoryInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(new Address { Line1 = "Some House" })
                    .ToANew<Address>(cfg => cfg
                        .CreateInstancesUsing(ctx => new Address { Line2 = "Some Street" })
                        .And
                        .Ignore(a => a.Line2));

                result1.Line1.ShouldBe("Some House");
                result1.Line2.ShouldBe("Some Street");

                var result2 = mapper
                    .Map(new Address { Line1 = "Some Other House" })
                    .ToANew<Address>(cfg => cfg
                        .CreateInstancesUsing(ctx => new Address { Line2 = "Some Street" })
                        .And
                        .Ignore(a => a.Line2));

                result2.Line1.ShouldBe("Some Other House");
                result2.Line2.ShouldBe("Some Street");

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldReplaceATargetInstanceFactory()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .ToANew<Address>()
                    .CreateInstancesUsing(ctx => new Address { Line2 = "Line 2!" })
                    .And
                    .Ignore(a => a.Line2);

                var result = mapper
                    .Map(new Address { Line1 = "Line 1!" })
                    .ToANew<Address>(cfg => cfg
                        .CreateInstancesUsing(ctx => new Address { Line2 = "Line 2?!" }));

                result.Line1.ShouldBe("Line 1!");
                result.Line2.ShouldBe("Line 2?!");
            }
        }

        [Fact]
        public void ShouldReplaceATargetInstanceFactoryForANestedObject()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Over<Address>()
                    .CreateInstancesUsing(ctx => new Address { Line2 = "Line 2!" })
                    .And
                    .Ignore(a => a.Line2);

                var result = mapper
                    .Map(new CustomerViewModel { Name = "Me", AddressLine1 = "Line 1!" })
                    .Over(new Customer { Name = "You" }, cfg => cfg
                           .CreateInstancesOf<Address>().Using(ctx => new Address { Line2 = "Line 2?!" }));

                result.Name.ShouldBe("Me");
                result.Address.ShouldNotBeNull();
                result.Address.Line1.ShouldBe("Line 1!");
                result.Address.Line2.ShouldBe("Line 2?!");
            }
        }

        [Fact]
        public void ShouldExtendTargetInstanceFactories()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .OnTo<Product>()
                    .CreateInstancesUsing(ctx => new Product { ProductId = ctx.Source.GetType().Name });

                Func<ProductDto> dtoFactory = () => new ProductDto { ProductId = "Product DTO" };

                var result1 = mapper
                    .Map(new PublicTwoFields<Product, ProductDto>
                    {
                        Value1 = new Product { Price = 9.99 },
                        Value2 = new ProductDto { Price = 0.99m }
                    })
                    .OnTo(new PublicTwoFields<ProductDto, Product>(), cfg => cfg
                        .CreateInstancesOf<ProductDto>().Using(dtoFactory));

                result1.Value1.ProductId.ShouldBe("Product DTO");
                result1.Value1.Price.ShouldBe(9.99m);
                result1.Value2.ProductId.ShouldBe("ProductDto");
                result1.Value2.Price.ShouldBe(0.99);

                var result2 = mapper
                    .Map(new PublicTwoFields<Product, ProductDto>
                    {
                        Value1 = new Product { Price = 6.66 },
                        Value2 = new ProductDto { Price = 1.01m }
                    })
                    .OnTo(new PublicTwoFields<ProductDto, Product>(), cfg => cfg
                        .CreateInstancesOf<ProductDto>().Using(dtoFactory));

                result2.Value1.ProductId.ShouldBe("Product DTO");
                result2.Value1.Price.ShouldBe(6.66m);
                result2.Value2.ProductId.ShouldBe("ProductDto");
                result2.Value2.Price.ShouldBe(1.01);

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldErrorIfDuplicateTargetInstanceFactoryIsConfigured()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Over<Product>()
                    .CreateInstancesUsing(ctx => new Product { ProductId = "La la la" });

                var factoryEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping
                        .Over<Product>()
                        .CreateInstancesUsing(ctx => new Product { ProductId = "La la la" });
                });

                factoryEx.Message.ShouldContain("already been configured");
            }
        }
    }
}
