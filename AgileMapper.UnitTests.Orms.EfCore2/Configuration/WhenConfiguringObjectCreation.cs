namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringObjectCreation : WhenConfiguringObjectCreation<EfCore2TestDbContext>
    {
        public WhenConfiguringObjectCreation(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUseAConditionalObjectFactory() => RunShouldUseAConditionalObjectFactory();
    }
}
