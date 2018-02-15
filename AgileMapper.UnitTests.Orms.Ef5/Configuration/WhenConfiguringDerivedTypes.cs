namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringDerivedTypes : WhenConfiguringDerivedTypes<Ef5TestDbContext>
    {
        public WhenConfiguringDerivedTypes(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingToConfiguredDerivedTypes() => RunShouldErrorProjectingToConfiguredDerivedTypes();

        [Fact]
        public Task ShouldErrorProjectingToAFallbackDerivedType() => RunShouldErrorProjectingToAFallbackDerivedType();

        [Fact]
        public Task ShouldErrorAttemptingToNotApplyDerivedSourceTypePairing()
            => RunShouldErrorAttemptingToNotApplyDerivedSourceTypePairing();
    }
}
