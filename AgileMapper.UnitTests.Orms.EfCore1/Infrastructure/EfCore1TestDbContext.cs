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

        public DbSet<Person> Persons { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Rota> Rotas { get; set; }

        public DbSet<RotaEntry> RotaEntries { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<PublicBool> BoolItems { get; set; }

        public DbSet<PublicShort> ShortItems { get; set; }

        public DbSet<PublicInt> IntItems { get; set; }

        public DbSet<PublicLong> LongItems { get; set; }

        public DbSet<PublicString> StringItems { get; set; }

        #region ITestDbContext Members

        IDbSetWrapper<Product> ITestDbContext.Products
            => new EfCore1DbSetWrapper<Product>(Products);

        IDbSetWrapper<Person> ITestDbContext.Persons
            => new EfCore1DbSetWrapper<Person>(Persons);

        IDbSetWrapper<Address> ITestDbContext.Addresses
            => new EfCore1DbSetWrapper<Address>(Addresses);

        IDbSetWrapper<Rota> ITestDbContext.Rotas
            => new EfCore1DbSetWrapper<Rota>(Rotas);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries
            => new EfCore1DbSetWrapper<RotaEntry>(RotaEntries);

        IDbSetWrapper<Order> ITestDbContext.Orders
            => new EfCore1DbSetWrapper<Order>(Orders);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems
            => new EfCore1DbSetWrapper<OrderItem>(OrderItems);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems
            => new EfCore1DbSetWrapper<PublicBool>(BoolItems);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems
            => new EfCore1DbSetWrapper<PublicShort>(ShortItems);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems
            => new EfCore1DbSetWrapper<PublicInt>(IntItems);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems
            => new EfCore1DbSetWrapper<PublicLong>(LongItems);

        IDbSetWrapper<PublicString> ITestDbContext.StringItems
            => new EfCore1DbSetWrapper<PublicString>(StringItems);

        void ITestDbContext.SaveChanges() => SaveChanges();

        #endregion
    }
}