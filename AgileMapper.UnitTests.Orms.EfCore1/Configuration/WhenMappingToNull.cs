namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenMappingToNull : WhenMappingToNull<EfCore1TestDbContext>
    {
        public WhenMappingToNull(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAUserConfiguration() => RunShouldApplyAUserConfiguration();
    }
}
