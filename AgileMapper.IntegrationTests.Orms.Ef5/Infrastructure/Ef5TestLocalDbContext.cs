namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.Infrastructure
{
    using System.Data.SqlClient;
    using UnitTests.Orms;
    using UnitTests.Orms.Ef5.Infrastructure;
    using UnitTests.Orms.Infrastructure;

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