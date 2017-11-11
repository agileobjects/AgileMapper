namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    public class LocalDbTestContext<TOrmContext> : ITestContext<TOrmContext>
        where TOrmContext : ITestLocalDbContext, new()
    {
        public LocalDbTestContext()
        {
            DbContext = new TOrmContext();
            DbContext.CreateDatabase();
        }

        public TOrmContext DbContext { get; }

        public void Dispose()
        {
            DbContext.DeleteDatabase();
            DbContext.Dispose();
        }
    }
}