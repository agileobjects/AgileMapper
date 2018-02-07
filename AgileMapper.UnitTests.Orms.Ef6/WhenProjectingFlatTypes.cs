namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms;
    using Xunit;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<Ef6TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingStructCtorParameters()
            => RunShouldErrorProjectingStructCtorParameters();
    }
}