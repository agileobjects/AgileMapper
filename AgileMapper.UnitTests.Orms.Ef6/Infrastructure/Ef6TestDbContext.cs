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

        public DbSet<Company> Companies { get; set; }

        public DbSet<Employee> Employees { get; set; }

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

        IDbSetWrapper<Company> ITestDbContext.Companies
            => new Ef6DbSetWrapper<Company>(Companies);

        IDbSetWrapper<Employee> ITestDbContext.Employees
            => new Ef6DbSetWrapper<Employee>(Employees);

        IDbSetWrapper<Product> ITestDbContext.Products
            => new Ef6DbSetWrapper<Product>(Products);

        IDbSetWrapper<Person> ITestDbContext.Persons
            => new Ef6DbSetWrapper<Person>(Persons);

        IDbSetWrapper<Address> ITestDbContext.Addresses
            => new Ef6DbSetWrapper<Address>(Addresses);

        IDbSetWrapper<Rota> ITestDbContext.Rotas
            => new Ef6DbSetWrapper<Rota>(Rotas);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries
            => new Ef6DbSetWrapper<RotaEntry>(RotaEntries);

        IDbSetWrapper<Order> ITestDbContext.Orders
            => new Ef6DbSetWrapper<Order>(Orders);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems
            => new Ef6DbSetWrapper<OrderItem>(OrderItems);

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