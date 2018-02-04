namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Threading.Tasks;
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOptional(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId);

            base.OnModelCreating(modelBuilder);
        }

        #region ITestDbContext Members

        IDbSetWrapper<Company> ITestDbContext.Companies
            => new Ef5DbSetWrapper<Company>(Companies);

        IDbSetWrapper<Employee> ITestDbContext.Employees
            => new Ef5DbSetWrapper<Employee>(Employees);

        IDbSetWrapper<Category> ITestDbContext.Categories
            => new Ef5DbSetWrapper<Category>(Categories);

        IDbSetWrapper<Product> ITestDbContext.Products
            => new Ef5DbSetWrapper<Product>(Products);

        IDbSetWrapper<Person> ITestDbContext.Persons
            => new Ef5DbSetWrapper<Person>(Persons);

        IDbSetWrapper<Address> ITestDbContext.Addresses
            => new Ef5DbSetWrapper<Address>(Addresses);

        IDbSetWrapper<Rota> ITestDbContext.Rotas
            => new Ef5DbSetWrapper<Rota>(Rotas);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries
            => new Ef5DbSetWrapper<RotaEntry>(RotaEntries);

        IDbSetWrapper<Order> ITestDbContext.Orders
            => new Ef5DbSetWrapper<Order>(Orders);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems
            => new Ef5DbSetWrapper<OrderItem>(OrderItems);

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

        Task ITestDbContext.SaveChanges()
        {
            SaveChanges();

            return Task.CompletedTask;
        }

        #endregion
    }
}