﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenConfiguringConstructorDataSources<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringConstructorDataSources(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        #region Project -> Ctor Parameter by Type

        protected Task RunShouldApplyAConfiguredConstantByParameterType()
            => RunTest(DoShouldApplyAConfiguredConstantByParameterType);

        protected Task RunShouldErrorApplyingAConfiguredConstantByParameterType()
            => RunTestAndExpectThrow(DoShouldApplyAConfiguredConstantByParameterType);

        private static async Task DoShouldApplyAConfiguredConstantByParameterType(TOrmContext context)
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
        }

        #endregion

        #region Project -> Ctor Parameter by Name
        protected Task RunShouldApplyAConfiguredExpressionByParameterName()
            => RunTest(DoShouldApplyAConfiguredExpressionByParameterName);

        protected Task RunShouldErrorApplyingAConfiguredExpressionByParameterName()
            => RunTestAndExpectThrow(DoShouldApplyAConfiguredExpressionByParameterName);

        private static async Task DoShouldApplyAConfiguredExpressionByParameterName(TOrmContext context)
        {
            var product = new Product { Name = "Prod.One" };

            context.Products.Add(product);
            await context.SaveChanges();

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .ProjectedTo<ProductDtoStruct>()
                    .Map(p => "2 * 3 = " + (2 * 3))
                    .ToCtor("name");

                var productDto = context
                    .Products
                    .ProjectUsing(mapper).To<ProductDtoStruct>()
                    .ShouldHaveSingleItem();

                productDto.ProductId.ShouldBe(product.ProductId);
                productDto.Name.ShouldBe("2 * 3 = 6");
            }
        }

        #endregion
    }
}
