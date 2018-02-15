namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using Infrastructure;
    using Orms.Configuration.Inline;

    public class WhenConfiguringDataSourcesInline
        : WhenConfiguringDataSourcesInline<EfCore2TestDbContext>
    {
        public WhenConfiguringDataSourcesInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}