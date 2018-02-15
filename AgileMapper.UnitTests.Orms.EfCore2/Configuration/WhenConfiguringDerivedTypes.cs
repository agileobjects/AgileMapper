namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringDerivedTypes : WhenConfiguringDerivedTypes<EfCore2TestDbContext>
    {
        public WhenConfiguringDerivedTypes(InMemoryEfCore2TestContext context)
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
