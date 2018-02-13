namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringCallbacks : WhenConfiguringCallbacks<Ef5TestDbContext>
    {
        public WhenConfiguringCallbacks(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}
