namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms;
    using Xunit;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<EfCore2TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectStructCtorParameters() => RunShouldProjectStructCtorParameters();
    }
}