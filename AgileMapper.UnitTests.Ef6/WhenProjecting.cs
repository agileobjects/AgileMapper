namespace AgileObjects.AgileMapper.UnitTests.Ef6
{
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenProjecting : Ef6TestClassBase
    {
        public WhenProjecting(TestContext context)
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

                var products = context.Products.ProjectTo<ProductDto>().ToArray();

                products.Length.ShouldBe(2);
            });
        }
    }
}