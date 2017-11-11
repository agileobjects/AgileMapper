namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.Infrastructure
{
    using System.Data.Entity.Infrastructure;
    using UnitTests.Orms.Infrastructure;

    public class Ef5LocalDbTestContext : ITestContext
    {
        private readonly Ef5TestDbContext _dbContext;

        public Ef5LocalDbTestContext()
        {
            var localDbConnectionFactory =
                new SqlConnectionFactory(@"Data Source=(local);Integrated Security=True;MultipleActiveResultSets=True");

            var localDbConnection = localDbConnectionFactory.CreateConnection("Ef5TestDb");

            _dbContext = new Ef5TestDbContext(localDbConnection);

            _dbContext.Database.Create();
        }

        public TOrmContext GetDbContext<TOrmContext>()
            where TOrmContext : ITestDbContext, new()
        {
            return (TOrmContext)(object)_dbContext;
        }

        public void Dispose()
        {
            _dbContext.Database.Delete();
            _dbContext.Dispose();
        }
    }
}