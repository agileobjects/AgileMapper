namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Linq;
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

        protected Task DoShouldIgnoreAConfiguredMemberConditionally()
        {
            return RunTest(async context =>
            {
                var product1 = new Product { Name = "P.1" };
                var product2 = new Product { Name = "P.2" };

                context.Products.Add(product1);
                context.Products.Add(product2);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<ProductDto>()
                        .If(p => p.Name.EndsWith(2 + ""))
                        .Ignore(p => p.Name);

                    var productDtos = context
                        .Products
                        .ProjectUsing(mapper).To<ProductDto>()
                        .OrderBy(p => p.ProductId)
                        .ToArray();

                    productDtos.First().ProductId.ShouldBe(product1.ProductId);
                    productDtos.First().Name.ShouldBe("P.1");
                    productDtos.Second().ProductId.ShouldBe(product2.ProductId);
                    productDtos.Second().Name.ShouldBeNull();
                }
            });
        }
    }
}
