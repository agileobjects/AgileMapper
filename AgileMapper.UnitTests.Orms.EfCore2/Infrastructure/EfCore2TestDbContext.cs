namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;

    public class EfCore2TestDbContext : DbContext, ITestDbContext
    {
        private static readonly DbContextOptions _inMemoryOptions =
            new DbContextOptionsBuilder<EfCore2TestDbContext>()
            .UseInMemoryDatabase(databaseName: "Ef6TestDbContext")
            .Options;

        public EfCore2TestDbContext()
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

        IDbSetWrapper<Product> ITestDbContext.Products
            => new EfCore2DbSetWrapper<Product>(Products);

        IDbSetWrapper<PublicBoolProperty> ITestDbContext.BoolItems
            => new EfCore2DbSetWrapper<PublicBoolProperty>(BoolItems);

        IDbSetWrapper<PublicShortProperty> ITestDbContext.ShortItems
            => new EfCore2DbSetWrapper<PublicShortProperty>(ShortItems);

        IDbSetWrapper<PublicIntProperty> ITestDbContext.IntItems
            => new EfCore2DbSetWrapper<PublicIntProperty>(IntItems);

        IDbSetWrapper<PublicLongProperty> ITestDbContext.LongItems
            => new EfCore2DbSetWrapper<PublicLongProperty>(LongItems);

        IDbSetWrapper<PublicStringProperty> ITestDbContext.StringItems
            => new EfCore2DbSetWrapper<PublicStringProperty>(StringItems);

        void ITestDbContext.SaveChanges() => SaveChanges();

        #endregion
    }
}