namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenMappingToNull : WhenMappingToNull<Ef6TestDbContext>
    {
        public WhenMappingToNull(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAUserConfiguration() => RunShouldApplyAUserConfiguration();
    }
}
