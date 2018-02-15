namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenMappingToNull : WhenMappingToNull<Ef5TestDbContext>
    {
        public WhenMappingToNull(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorApplyingAUserConfiguration() => RunShouldErrorApplyingAUserConfiguration();
    }
}
