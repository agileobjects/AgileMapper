namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.Infrastructure
{
    using System.Data.SqlClient;
    using Ef6.Infrastructure;
    using Orms;
    using Orms.Infrastructure;

    public class Ef6TestLocalDbContext : Ef6TestDbContext, ITestLocalDbContext
    {
        public Ef6TestLocalDbContext()
            : base(new SqlConnection(TestConstants.GetLocalDbConnectionString<Ef6TestLocalDbContext>()))
        {
        }

        public override bool StringToDateTimeConversionSupported => true;

        public override bool StringToDateTimeValidationSupported => true;

        void ITestLocalDbContext.CreateDatabase() => Database.Create();

        void ITestLocalDbContext.DeleteDatabase() => Database.Delete();
    }
}