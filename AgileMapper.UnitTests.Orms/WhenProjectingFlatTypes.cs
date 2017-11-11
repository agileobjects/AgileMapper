namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Linq;
    using Infrastructure;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public abstract class WhenProjectingFlatTypes<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenProjectingFlatTypes(ITestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAFlatTypeToAnArray()
        {
            RunTest(context =>
            {
                context.Products.Add(new Product
                {
                    ProductId = 1,
                    Name = "Product One"
                });

                context.Products.Add(new Product
                {
                    ProductId = 2,
                    Name = "Product Two"
                });

                context.SaveChanges();

                var products = context.Products.ToArray();
                var productDtos = context.Products.ProjectTo<ProductDto>().ToArray();

                productDtos.Length.ShouldBe(2);

                productDtos[0].ProductId.ShouldBe(products[0].ProductId);
                productDtos[0].Name.ShouldBe(products[0].Name);

                productDtos[1].ProductId.ShouldBe(products[1].ProductId);
                productDtos[1].Name.ShouldBe(products[1].Name);
            });
        }
    }
}