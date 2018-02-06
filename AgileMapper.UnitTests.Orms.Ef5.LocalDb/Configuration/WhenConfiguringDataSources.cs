namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Orms.Infrastructure;
    using Xunit;

    public class WhenConfiguringDataSources : WhenConfiguringDataSources<Ef5TestLocalDbContext>
    {
        public WhenConfiguringDataSources(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }

        // Executed in LocalDb because the configured conditions require string conversions

        [Fact]
        public Task ShouldApplyAConfiguredMember() => DoShouldApplyAConfiguredMember();

        [Fact]
        public Task ShouldApplyMultipleConfiguredMembers() => DoShouldApplyMultipleConfiguredMembers();
    }
}