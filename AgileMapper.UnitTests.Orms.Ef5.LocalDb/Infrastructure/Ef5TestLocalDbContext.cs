namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.Infrastructure
{
    using System.Data.SqlClient;
    using Ef5.Infrastructure;
    using Orms;
    using Orms.Infrastructure;

    public class Ef5TestLocalDbContext : Ef5TestDbContext, ITestLocalDbContext
    {
        public Ef5TestLocalDbContext()
            : base(new SqlConnection(TestConstants.GetLocalDbConnectionString<Ef5TestLocalDbContext>()))
        {
        }

        void ITestLocalDbContext.CreateDatabase() => Database.Create();

        void ITestLocalDbContext.DeleteDatabase() => Database.Delete();
    }
}