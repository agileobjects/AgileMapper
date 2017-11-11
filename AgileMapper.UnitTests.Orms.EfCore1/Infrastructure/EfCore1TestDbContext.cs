namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;

    public class EfCore1TestDbContext : DbContext, ITestDbContext
    {
        private static readonly DbContextOptions _inMemoryOptions =
            new DbContextOptionsBuilder<EfCore1TestDbContext>()
                .UseInMemoryDatabase(databaseName: "EfCore1TestDb")
                .Options;

        public EfCore1TestDbContext()
            : base(_inMemoryOptions)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<PublicBoolProperty> BoolItems { get; set; }

        public DbSet<PublicShortProperty> ShortItems { get; set; }

        public DbSet<PublicIntProperty> IntItems { get; set; }

        public DbSet<PublicLongProperty> LongItems { get; set; }

        public DbSet<PublicStringProperty> StringItems { get; set; }

        #region ITestDbContext Members

        public bool StringParsingSupported => true;

        IDbSetWrapper<Product> ITestDbContext.Products
            => new EfCore1DbSetWrapper<Product>(Products);

        IDbSetWrapper<PublicBoolProperty> ITestDbContext.BoolItems
            => new EfCore1DbSetWrapper<PublicBoolProperty>(BoolItems);

        IDbSetWrapper<PublicShortProperty> ITestDbContext.ShortItems
            => new EfCore1DbSetWrapper<PublicShortProperty>(ShortItems);

        IDbSetWrapper<PublicIntProperty> ITestDbContext.IntItems
            => new EfCore1DbSetWrapper<PublicIntProperty>(IntItems);

        IDbSetWrapper<PublicLongProperty> ITestDbContext.LongItems
            => new EfCore1DbSetWrapper<PublicLongProperty>(LongItems);

        IDbSetWrapper<PublicStringProperty> ITestDbContext.StringItems
            => new EfCore1DbSetWrapper<PublicStringProperty>(StringItems);

        void ITestDbContext.SaveChanges() => SaveChanges();

        #endregion
    }
}