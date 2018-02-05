namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringDataSources : WhenConfiguringDataSources<Ef5TestDbContext>
    {
        public WhenConfiguringDataSources(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstant()
            => DoShouldApplyAConfiguredConstant();
    }
}