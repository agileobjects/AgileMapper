namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringDataSourcesInline<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringDataSourcesInline(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantInline()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "P1" };

                await context.Products.Add(product);
                await context.SaveChanges();

                var productDto = context
                    .Products
                    .Project().To<ProductDto>(cfg => cfg
                        .Map("PROD!!")
                        .To(dto => dto.Name))
                    .ShouldHaveSingleItem();

                productDto.ProductId.ShouldBe(product.ProductId);
                productDto.Name.ShouldBe("PROD!!");
            });
        }
    }
}
