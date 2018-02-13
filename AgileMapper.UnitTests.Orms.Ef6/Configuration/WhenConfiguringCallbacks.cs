namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringCallbacks : WhenConfiguringCallbacks<Ef6TestDbContext>
    {
        public WhenConfiguringCallbacks(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}
