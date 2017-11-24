namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenViewingMappingPlans
    {
        [Fact]
        public void ShouldCreateAProjectionMappingPlan()
        {
            string plan = Mapper
                .GetPlanFor<Product>()
                .ProjectedTo<ProductDto>();

            plan.ShouldContain("Rule Set: Project");
            plan.ShouldContain("Source.Select(");
            plan.ShouldContain("new ProductDto");

            var cachedMapper = Mapper.Default.Context.ObjectMapperFactory.RootMappers.ShouldHaveSingleItem();

            cachedMapper.MapperData.SourceType.ShouldBe(typeof(IQueryable<Product>));
            cachedMapper.MapperData.TargetType.ShouldBe(typeof(IQueryable<ProductDto>));
        }
    }
}
