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
            => new EfCore1DbSetWrapper<Company>(Companies);

        IDbSetWrapper<Employee> ITestDbContext.Employees
            => new EfCore1DbSetWrapper<Employee>(Employees);

        IDbSetWrapper<Category> ITestDbContext.Categories
            => new EfCore1DbSetWrapper<Category>(Categories);

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