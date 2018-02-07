namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringConstructorDataSources<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringConstructorDataSources(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantByParameterType()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "Prod.One" };

                context.Products.Add(product);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<ProductDtoStruct>()
                        .Map("Bananas!")
                        .ToCtor<string>();

                    var productDto = context
                        .Products
                        .ProjectUsing(mapper).To<ProductDtoStruct>()
                        .ShouldHaveSingleItem();

                    productDto.ProductId.ShouldBe(product.ProductId);
                    productDto.Name.ShouldBe("Bananas!");
                }
            });
        }
    }
}
