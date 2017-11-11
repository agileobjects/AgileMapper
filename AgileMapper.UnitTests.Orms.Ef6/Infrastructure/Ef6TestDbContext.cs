namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity;
    using Effort;
    using Orms.Infrastructure;
    using TestClasses;

    public class Ef6TestDbContext : DbContext, ITestDbContext
    {
        public Ef6TestDbContext()
            : this(DbConnectionFactory.CreateTransient())
        {
        }

        protected Ef6TestDbContext(DbConnection dbConnection)
            : base(dbConnection, true)
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
            => new Ef6DbSetWrapper<Product>(Products);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems
            => new Ef6DbSetWrapper<PublicBool>(BoolItems);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems
            => new Ef6DbSetWrapper<PublicShort>(ShortItems);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems
            => new Ef6DbSetWrapper<PublicInt>(IntItems);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems
            => new Ef6DbSetWrapper<PublicLong>(LongItems);

        IDbSetWrapper<PublicString> ITestDbContext.StringItems
            => new Ef6DbSetWrapper<PublicString>(StringItems);

        void ITestDbContext.SaveChanges() => SaveChanges();

        #endregion
    }
}