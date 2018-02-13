namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringObjectCreation : WhenConfiguringObjectCreation<Ef6TestDbContext>
    {
        public WhenConfiguringObjectCreation(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorUsingAConditionalObjectFactory() => RunShouldErrorUsingAConditionalObjectFactory();
    }
}
