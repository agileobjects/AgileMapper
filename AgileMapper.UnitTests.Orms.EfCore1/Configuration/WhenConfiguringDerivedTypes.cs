namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringDerivedTypes : WhenConfiguringDerivedTypes<EfCore1TestDbContext>
    {
        public WhenConfiguringDerivedTypes(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToConfiguredDerivedTypes() => RunShouldProjectToConfiguredDerivedTypes();

        [Fact]
        public Task ShouldProjectToAFallbackDerivedType() => RunShouldProjectToAFallbackDerivedType();

        [Fact]
        public Task ShouldNotAttemptToApplyDerivedSourceTypePairing() => RunShouldNotAttemptToApplyDerivedSourceTypePairing();
    }
}
