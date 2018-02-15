namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringObjectCreation : WhenConfiguringObjectCreation<EfCore1TestDbContext>
    {
        public WhenConfiguringObjectCreation(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUseAConditionalObjectFactory() => RunShouldUseAConditionalObjectFactory();
    }
}
