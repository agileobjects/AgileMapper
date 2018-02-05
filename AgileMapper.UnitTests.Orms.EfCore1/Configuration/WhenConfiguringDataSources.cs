namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringDataSources : WhenConfiguringDataSources<EfCore1TestDbContext>
    {
        public WhenConfiguringDataSources(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstant()
            => DoShouldApplyAConfiguredConstant();
    }
}