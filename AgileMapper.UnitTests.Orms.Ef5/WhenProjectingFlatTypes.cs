namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms;
    using Xunit;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<Ef5TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingStructCtorParameters()
            => RunShouldErrorProjectingStructCtorParameters();
    }
}