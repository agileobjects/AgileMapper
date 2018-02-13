namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringObjectCreation : WhenConfiguringObjectCreation<EfCore2TestDbContext>
    {
        public WhenConfiguringObjectCreation(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
