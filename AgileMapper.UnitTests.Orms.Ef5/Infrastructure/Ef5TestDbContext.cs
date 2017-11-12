namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity;
    using Effort;
    using Orms.Infrastructure;
    using TestClasses;

    public class Ef5TestDbContext : DbContext, ITestDbContext
    {
        public Ef5TestDbContext()
            : this(DbConnectionFactory.CreateTransient())
        {
        }

        protected Ef5TestDbContext(DbConnection dbConnection)
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

        public virtual bool StringToDateTimeConversionSupported => false;

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
    }
}