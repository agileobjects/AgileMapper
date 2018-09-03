namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembersInline : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenIgnoringMembersInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldIgnoreAConfiguredMemberConditionallyInline()
        {
            return RunTest(async (context, mapper) =>
            {
                var product1 = new Product { Name = "1" };
                var product2 = new Product { Name = "P.2" };

                await context.Products.AddRangeAsync(product1, product2);
                await context.SaveChangesAsync();

                var productDtos = context
                    .Products
                    .ProjectUsing(mapper).To<ProductDto>(cfg => cfg
                        .If(p => p.Name.Length < 2)
                        .Ignore(p => p.Name))
                    .OrderBy(p => p.ProductId)
                    .ToArray();

                productDtos.First().ProductId.ShouldBe(product1.ProductId);
                productDtos.First().Name.ShouldBeNull();
                productDtos.Second().ProductId.ShouldBe(product2.ProductId);
                productDtos.Second().Name.ShouldBe("P.2");
            });
        }
    }
}
