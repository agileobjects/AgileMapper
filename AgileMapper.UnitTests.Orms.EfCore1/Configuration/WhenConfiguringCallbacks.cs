namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringCallbacks : WhenConfiguringCallbacks<EfCore1TestDbContext>
    {
        public WhenConfiguringCallbacks(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
