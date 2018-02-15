namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenMappingToNull : WhenMappingToNull<EfCore2TestDbContext>
    {
        public WhenMappingToNull(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAUserConfiguration() => RunShouldApplyAUserConfiguration();
    }
}
