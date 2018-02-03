namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Linq;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenProjectingFlatTypes<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenProjectingFlatTypes(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAFlatTypeToAnArray()
        {
            RunTest(context =>
            {
                var product1 = new Product { Name = "Product One" };
                var product2 = new Product { Name = "Product Two" };

                context.Products.Add(product1);
                context.Products.Add(product2);
                context.SaveChanges();

                var productDtos = context
                    .Products
                    .Project().To<ProductDto>()
                    .OrderBy(p => p.ProductId)
                    .ToArray();

                productDtos.Length.ShouldBe(2);

                productDtos[0].ProductId.ShouldBe(product1.ProductId);
                productDtos[0].Name.ShouldBe(product1.Name);

                productDtos[1].ProductId.ShouldBe(product2.ProductId);
                productDtos[1].Name.ShouldBe(product2.Name);
            });
        }

        [Fact]
        public void ShouldProjectAFlatTypeToANonMatchingTypeList()
        {
            RunTest(context =>
            {
                var product = new Product { Name = "Uno" };

                context.Products.Add(product);
                context.SaveChanges();

                var productDtos = context.Products.Project().To<PublicStringDto>().ToList();

                productDtos.ShouldHaveSingleItem();
                productDtos[0].Id.ShouldBe(product.ProductId);
                productDtos[0].Value.ShouldBeNull();
            });
        }
    }
}