namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringObjectCreation : WhenConfiguringObjectCreation<Ef5TestDbContext>
    {
        public WhenConfiguringObjectCreation(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorUsingAConditionalObjectFactory() => RunShouldErrorUsingAConditionalObjectFactory();
    }
}
