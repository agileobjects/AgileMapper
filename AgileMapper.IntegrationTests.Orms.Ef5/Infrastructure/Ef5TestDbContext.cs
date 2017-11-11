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

        public DbSet<PublicBoolProperty> BoolItems { get; set; }

        public DbSet<PublicShortProperty> ShortItems { get; set; }

        public DbSet<PublicIntProperty> IntItems { get; set; }

        public DbSet<PublicLongProperty> LongItems { get; set; }

        public DbSet<PublicStringProperty> StringItems { get; set; }

        #region ITestDbContext Members

        public bool StringParsingSupported => false;

        IDbSetWrapper<Product> ITestDbContext.Products
            => new Ef5DbSetWrapper<Product>(Products);

        IDbSetWrapper<PublicBoolProperty> ITestDbContext.BoolItems
            => new Ef5DbSetWrapper<PublicBoolProperty>(BoolItems);

        IDbSetWrapper<PublicShortProperty> ITestDbContext.ShortItems
            => new Ef5DbSetWrapper<PublicShortProperty>(ShortItems);

        IDbSetWrapper<PublicIntProperty> ITestDbContext.IntItems
            => new Ef5DbSetWrapper<PublicIntProperty>(IntItems);

        IDbSetWrapper<PublicLongProperty> ITestDbContext.LongItems
            => new Ef5DbSetWrapper<PublicLongProperty>(LongItems);

        IDbSetWrapper<PublicStringProperty> ITestDbContext.StringItems
            => new Ef5DbSetWrapper<PublicStringProperty>(StringItems);

        void ITestDbContext.SaveChanges() => SaveChanges();

        #endregion

        #region ITestLocalDbContext Members

        void ITestLocalDbContext.CreateDatabase() => Database.Create();

        void ITestLocalDbContext.DeleteDatabase() => Database.Delete();

        #endregion
    }
}