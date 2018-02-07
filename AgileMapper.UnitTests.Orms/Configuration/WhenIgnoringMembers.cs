namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenIgnoringMembers<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenIgnoringMembers(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        protected Task DoShouldIgnoreAConfiguredMember()
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
                        .Ignore(p => p.Name);

                    var productDto = context
                        .Products
                        .ProjectUsing(mapper).To<ProductDto>()
                        .ShouldHaveSingleItem();

                    productDto.ProductId.ShouldBe(product.ProductId);
                    productDto.Name.ShouldBeNull();
                }
            });
        }
    }
}
