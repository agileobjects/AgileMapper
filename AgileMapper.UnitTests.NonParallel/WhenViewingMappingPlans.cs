namespace AgileObjects.AgileMapper.UnitTests.NonParallel
{
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
    using Xunit;

    public class WhenViewingMappingPlans : NonParallelTestsBase
    {
        [Fact]
        public void ShouldCreateAPlanSetViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                var plans = Mapper.GetPlansFor<Product>().To<ProductDto>().ToString();

                plans.ShouldContain("Rule Set: CreateNew");
                plans.ShouldContain("Rule Set: Overwrite");
                plans.ShouldContain("Rule Set: Merge");
            });
        }

        [Fact]
        public void ShouldShowAllCachedMappingPlansViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                Mapper.GetPlanFor<MysteryCustomer>().ToANew<MysteryCustomerViewModel>();
                Mapper.GetPlansFor(new MegaProduct()).To<ProductDtoMega>();

                string plan = Mapper.GetPlansInCache();

                plan.ShouldContain("MysteryCustomer -> MysteryCustomerViewModel");
                plan.ShouldContain("MegaProduct -> ProductDtoMega");
                plan.ShouldContain("Rule Set: CreateNew");
                plan.ShouldContain("Rule Set: Merge");
                plan.ShouldContain("Rule Set: Overwrite");
            });
        }

        [Fact]
        public void ShouldShowAllCachedMappingPlanExpressionsViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                Mapper.GetPlanFor<MysteryCustomer>().ToANew<MysteryCustomerViewModel>();
                Mapper.GetPlansFor(new MegaProduct()).To<ProductDtoMega>();

                var plan = Mapper.GetPlanExpressionsInCache();

                plan.ShouldNotBeNull().Any().ShouldBeTrue();
            });
        }
    }
}
