namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration.Inline
{
    using Infrastructure;
    using Orms.Configuration.Inline;

    public class WhenConfiguringDataSourcesInline
        : WhenConfiguringDataSourcesInline<Ef5TestDbContext>
    {
        public WhenConfiguringDataSourcesInline(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}