namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringDataSources : WhenConfiguringDataSources<Ef5TestDbContext>
    {
        public WhenConfiguringDataSources(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}