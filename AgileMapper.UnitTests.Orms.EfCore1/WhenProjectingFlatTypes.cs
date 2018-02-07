namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms;
    using Xunit;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<EfCore1TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectStructCtorParameters() => RunShouldProjectStructCtorParameters();
    }
}