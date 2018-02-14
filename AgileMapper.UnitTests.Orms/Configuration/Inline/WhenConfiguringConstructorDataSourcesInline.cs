namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenConfiguringConstructorDataSourcesInline<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringConstructorDataSourcesInline(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        #region Project -> Ctor Parameter by Type Inline

        protected Task RunShouldApplyAConfiguredConstantByParameterTypeInline()
            => RunTest(DoShouldApplyAConfiguredConstantByParameterType);

        protected Task RunShouldErrorApplyingAConfiguredConstantByParameterTypeInline()
            => RunTestAndExpectThrow(DoShouldApplyAConfiguredConstantByParameterType);

        private static async Task DoShouldApplyAConfiguredConstantByParameterType(TOrmContext context, IMapper mapper)
        {
            var product = new Product { Name = "Prod.1" };

            await context.Products.Add(product);
            await context.SaveChanges();

            var productDto = context
                .Products
                .ProjectUsing(mapper).To<ProductStruct>(cfg => cfg
                    .Map("GRAPES!")
                    .ToCtor<string>())
                .ShouldHaveSingleItem();

            productDto.ProductId.ShouldBe(product.ProductId);
            productDto.Name.ShouldBe("GRAPES!");
        }

        #endregion
    }
}
