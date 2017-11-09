namespace AgileObjects.AgileMapper.UnitTests.Ef6.Infrastructure
{
    using System.Data.Entity;
    using Effort;
    using Orms.Infrastructure;
    using Orms.TestClasses;

    public class Ef6TestDbContext : DbContext, ITestDbContext
    {
        public Ef6TestDbContext()
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
            => new Ef6DbSetWrapper<Product>(Products);

        IDbSetWrapper<PublicBoolProperty> ITestDbContext.BoolItems
            => new Ef6DbSetWrapper<PublicBoolProperty>(BoolItems);

        IDbSetWrapper<PublicShortProperty> ITestDbContext.ShortItems
            => new Ef6DbSetWrapper<PublicShortProperty>(ShortItems);

        IDbSetWrapper<PublicIntProperty> ITestDbContext.IntItems
            => new Ef6DbSetWrapper<PublicIntProperty>(IntItems);

        IDbSetWrapper<PublicLongProperty> ITestDbContext.LongItems
            => new Ef6DbSetWrapper<PublicLongProperty>(LongItems);

        IDbSetWrapper<PublicStringProperty> ITestDbContext.StringItems
            => new Ef6DbSetWrapper<PublicStringProperty>(StringItems);

        void ITestDbContext.SaveChanges() => SaveChanges();

        #endregion
    }
}