namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration.Inline
{
    using Infrastructure;
    using Orms.Configuration.Inline;

    public class WhenConfiguringDataSourcesInline
        : WhenConfiguringDataSourcesInline<EfCore1TestDbContext>
    {
        public WhenConfiguringDataSourcesInline(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}