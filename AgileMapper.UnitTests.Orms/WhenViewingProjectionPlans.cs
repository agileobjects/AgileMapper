namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using MoreTestClasses;
    using ObjectPopulation;
    using TestClasses;
    using Xunit;

    public abstract class WhenViewingProjectionPlans<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenViewingProjectionPlans(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldCreateAQueryProjectionPlanForASpecificQueryProvider()
        {
            return RunTest(mapper =>
            {
                string plan = mapper
                    .GetPlanForProjecting(Context.Products)
                    .To<ProductDto>();

                plan.ShouldContain("Rule Set: Project");
                plan.ShouldContain("Source.Select(");
                plan.ShouldContain("new ProductDto");

                var cachedMapper = (IObjectMapper)mapper.RootMapperCountShouldBeOne();

                cachedMapper.MapperData.SourceType.ShouldBe(typeof(IQueryable<Product>));
                cachedMapper.MapperData.TargetType.ShouldBe(typeof(IQueryable<ProductDto>));

                // Trigger a mapping:
                Context.Products.Project().To<ProductDto>().ShouldBeEmpty();

                var usedMapper = (IObjectMapper)mapper.RootMapperCountShouldBeOne();

                usedMapper.ShouldBe(cachedMapper);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task ShouldReturnCachedQueryProjectionPlansInAllCachedPlans()
        {
            return RunTest((IMapper mapper) =>
            {
                try
                {
                    Mapper.GetPlanForProjecting(Context.Products).To<ProductDto>();
                    Mapper.GetPlanForProjecting(Context.StringItems).To<PublicStringDto>();
                    Mapper.GetPlanForProjecting(Context.Persons).To<PersonViewModel>();

                    var allPlans = Mapper.GetPlansInCache();

                    allPlans.ShouldContain("IQueryable<Product> -> IQueryable<ProductDto>");
                    allPlans.ShouldContain("IQueryable<PublicString> -> IQueryable<PublicStringDto>");
                    allPlans.ShouldContain("IQueryable<Person> -> IQueryable<PersonViewModel>");

                    return Task.CompletedTask;
                }
                finally
                {
                    Mapper.ResetDefaultInstance();
                }
            });
        }
    }
}
