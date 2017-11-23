﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Debug;
    using Orms.Infrastructure;
    using TestClasses;

    public class EfCore2TestDbContext : DbContext, ITestDbContext
    {
        private static readonly DbContextOptions _inMemoryOptions =
            new DbContextOptionsBuilder<EfCore2TestDbContext>()
                .UseLoggerFactory(new LoggerFactory(new[] { new DebugLoggerProvider() }))
                .UseInMemoryDatabase(databaseName: "EfCore2TestDb")
                .Options;

        public EfCore2TestDbContext()
            : this(_inMemoryOptions)
        {
        }

        protected EfCore2TestDbContext(DbContextOptions options)
            : base(options)
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
            => new EfCore2DbSetWrapper<Product>(Products);

        IDbSetWrapper<Person> ITestDbContext.Persons
            => new EfCore2DbSetWrapper<Person>(Persons);

        IDbSetWrapper<Address> ITestDbContext.Addresses
            => new EfCore2DbSetWrapper<Address>(Addresses);

        IDbSetWrapper<Rota> ITestDbContext.Rotas
            => new EfCore2DbSetWrapper<Rota>(Rotas);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries
            => new EfCore2DbSetWrapper<RotaEntry>(RotaEntries);

        IDbSetWrapper<Order> ITestDbContext.Orders
            => new EfCore2DbSetWrapper<Order>(Orders);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems
            => new EfCore2DbSetWrapper<OrderItem>(OrderItems);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems
            => new EfCore2DbSetWrapper<PublicBool>(BoolItems);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems
            => new EfCore2DbSetWrapper<PublicShort>(ShortItems);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems
            => new EfCore2DbSetWrapper<PublicInt>(IntItems);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems
            => new EfCore2DbSetWrapper<PublicLong>(LongItems);

        IDbSetWrapper<PublicString> ITestDbContext.StringItems
            => new EfCore2DbSetWrapper<PublicString>(StringItems);

        void ITestDbContext.SaveChanges() => SaveChanges();

        #endregion
    }
}