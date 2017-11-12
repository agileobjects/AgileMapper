namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef6.Infrastructure
{
    using System.Data.SqlClient;
    using UnitTests.Orms;
    using UnitTests.Orms.Ef6.Infrastructure;
    using UnitTests.Orms.Infrastructure;

    public class Ef6TestLocalDbContext : Ef6TestDbContext, ITestLocalDbContext
    {
        public Ef6TestLocalDbContext()
            : base(new SqlConnection(TestConstants.GetLocalDbConnectionString<Ef6TestLocalDbContext>()))
        {
        }

        public override bool StringToDateTimeConversionSupported => true;

        void ITestLocalDbContext.CreateDatabase() => Database.Create();

        void ITestLocalDbContext.DeleteDatabase() => Database.Delete();
    }
}