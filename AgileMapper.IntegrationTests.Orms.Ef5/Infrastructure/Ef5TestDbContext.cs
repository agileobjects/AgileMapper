namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.Infrastructure
{
    using System.Data.Entity;
    using System.Data.SqlClient;
    using UnitTests.Orms;
    using UnitTests.Orms.Infrastructure;
    using UnitTests.Orms.TestClasses;

    public class Ef5TestDbContext : DbContext, ITestLocalDbContext
    {
        public Ef5TestDbContext()
            : base(
                new SqlConnection(TestConstants.GetLocalDbConnectionString<Ef5TestDbContext>()),
                true)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<PublicBool> BoolItems { get; set; }

        public DbSet<PublicShort> ShortItems { get; set; }

        public DbSet<PublicInt> IntItems { get; set; }

        public DbSet<PublicLong> LongItems { get; set; }

        public DbSet<PublicString> StringItems { get; set; }

        #region ITestDbContext Members

        public bool StringToNumberConversionSupported => false;

        IDbSetWrapper<Product> ITestDbContext.Products
            => new Ef5DbSetWrapper<Product>(Products);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems
            => new Ef5DbSetWrapper<PublicBool>(BoolItems);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems
            => new Ef5DbSetWrapper<PublicShort>(ShortItems);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems
            => new Ef5DbSetWrapper<PublicInt>(IntItems);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems
            => new Ef5DbSetWrapper<PublicLong>(LongItems);

        IDbSetWrapper<PublicString> ITestDbContext.StringItems
            => new Ef5DbSetWrapper<PublicString>(StringItems);

        void ITestDbContext.SaveChanges() => SaveChanges();

        #endregion

        #region ITestLocalDbContext Members

        void ITestLocalDbContext.CreateDatabase() => Database.Create();

        void ITestLocalDbContext.DeleteDatabase() => Database.Delete();

        #endregion
    }
}