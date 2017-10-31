namespace AgileObjects.AgileMapper.UnitTests.NonParallel
{
    using Shouldly;
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

                var plan = Mapper.GetPlansInCache();

                plan.ShouldContain("MysteryCustomer -> MysteryCustomerViewModel");
                plan.ShouldContain("MegaProduct -> ProductDtoMega");
                plan.ShouldContain("Rule set: CreateNew");
                plan.ShouldContain("Rule set: Merge");
                plan.ShouldContain("Rule set: Overwrite");
            });
        }
    }
}
