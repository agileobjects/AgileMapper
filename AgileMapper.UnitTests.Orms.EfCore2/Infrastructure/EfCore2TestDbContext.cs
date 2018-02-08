﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Infrastructure
{
    using System.Threading.Tasks;
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

        public DbSet<Company> Companies { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Category> Categories { get; set; }

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Company>()
                .HasOne(c => c.Ceo)
                .WithOne(e => e.Company)
                .HasForeignKey<Employee>(e => e.CompanyId);

            modelBuilder
                .Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId);

            base.OnModelCreating(modelBuilder);
        }

        #region ITestDbContext Members

        IDbSetWrapper<Company> ITestDbContext.Companies
            => new EfCore2DbSetWrapper<Company>(this);

        IDbSetWrapper<Employee> ITestDbContext.Employees
            => new EfCore2DbSetWrapper<Employee>(this);

        IDbSetWrapper<Category> ITestDbContext.Categories
            => new EfCore2DbSetWrapper<Category>(this);

        IDbSetWrapper<Product> ITestDbContext.Products
            => new EfCore2DbSetWrapper<Product>(this);

        IDbSetWrapper<Person> ITestDbContext.Persons
            => new EfCore2DbSetWrapper<Person>(this);

        IDbSetWrapper<Address> ITestDbContext.Addresses
            => new EfCore2DbSetWrapper<Address>(this);

        IDbSetWrapper<Rota> ITestDbContext.Rotas
            => new EfCore2DbSetWrapper<Rota>(this);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries
            => new EfCore2DbSetWrapper<RotaEntry>(this);

        IDbSetWrapper<Order> ITestDbContext.Orders
            => new EfCore2DbSetWrapper<Order>(this);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems
            => new EfCore2DbSetWrapper<OrderItem>(this);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems
            => new EfCore2DbSetWrapper<PublicBool>(this);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems
            => new EfCore2DbSetWrapper<PublicShort>(this);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems
            => new EfCore2DbSetWrapper<PublicInt>(this);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems
            => new EfCore2DbSetWrapper<PublicLong>(this);

        IDbSetWrapper<PublicString> ITestDbContext.StringItems
            => new EfCore2DbSetWrapper<PublicString>(this);

        Task ITestDbContext.SaveChanges() => SaveChangesAsync();

        #endregion
    }
}