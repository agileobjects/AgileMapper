namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    public class InMemoryOrmTestContext<TOrmContext> : ITestContext<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        public InMemoryOrmTestContext()
        {
            DbContext = new TOrmContext();
        }

        public TOrmContext DbContext { get; }

        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}