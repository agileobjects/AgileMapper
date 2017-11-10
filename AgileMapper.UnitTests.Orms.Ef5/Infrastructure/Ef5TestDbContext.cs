namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Infrastructure
{
    using System.Data.Entity;
    using Effort;
    using Orms.Infrastructure;
    using TestClasses;

    public class Ef5TestDbContext : DbContext, ITestDbContext
    {
        public Ef5TestDbContext()
            : base(DbConnectionFactory.CreateTransient(), true)
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
    }
}