namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration.Inline
{
    using Infrastructure;
    using Orms.Configuration.Inline;

    public class WhenConfiguringDataSourcesInline
        : WhenConfiguringDataSourcesInline<Ef6TestDbContext>
    {
        public WhenConfiguringDataSourcesInline(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}