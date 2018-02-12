namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringCallbacks : WhenConfiguringCallbacks<EfCore2TestDbContext>
    {
        public WhenConfiguringCallbacks(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
