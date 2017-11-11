namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;

    public class InMemoryOrmTestContext : ITestContext
    {
        private IDisposable _dbContext;

        public TOrmContext GetDbContext<TOrmContext>()
            where TOrmContext : ITestDbContext, new()
        {
            return (TOrmContext)(_dbContext ?? (_dbContext = new TOrmContext()));
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}