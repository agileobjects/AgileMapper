namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenConfiguringDataSources<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringDataSources(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        protected Task DoShouldApplyAConfiguredConstant()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "P1" };

                context.Products.Add(product);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<ProductDto>()
                        .Map("PRODUCT")
                        .To(dto => dto.Name);

                    var productDto = context
                        .Products
                        .Project(_ => _.Using(mapper)).To<ProductDto>()
                        .ShouldHaveSingleItem();

                    productDto.ProductId.ShouldBe(product.ProductId);
                    productDto.Name.ShouldBe("PRODUCT");
                }
            });
        }
    }
}