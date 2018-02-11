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

        public DbSet<Animal> Animals { get; set; }

        public DbSet<Shape> Shapes { get; set;  }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Person> Persons { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Rota> Rotas { get; set; }

        public DbSet<RotaEntry> RotaEntries { get; set; }

        public DbSet<OrderUk> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<PublicBool> BoolItems { get; set; }

        public DbSet<PublicByte> ByteItems { get; set; }

        public DbSet<PublicShort> ShortItems { get; set; }

        public DbSet<PublicInt> IntItems { get; set; }

        public DbSet<PublicLong> LongItems { get; set; }

        public DbSet<PublicString> StringItems { get; set; }

        public DbSet<PublicTitle> TitleItems { get; set; }

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

        IDbSetWrapper<Animal> ITestDbContext.Animals => new Ef5DbSetWrapper<Animal>(this);
        
        IDbSetWrapper<Shape> ITestDbContext.Shapes => new Ef5DbSetWrapper<Shape>(this);

        IDbSetWrapper<Company> ITestDbContext.Companies => new Ef5DbSetWrapper<Company>(this);

        IDbSetWrapper<Employee> ITestDbContext.Employees => new Ef5DbSetWrapper<Employee>(this);

        IDbSetWrapper<Category> ITestDbContext.Categories => new Ef5DbSetWrapper<Category>(this);

        IDbSetWrapper<Product> ITestDbContext.Products => new Ef5DbSetWrapper<Product>(this);

        IDbSetWrapper<Person> ITestDbContext.Persons => new Ef5DbSetWrapper<Person>(this);

        IDbSetWrapper<Address> ITestDbContext.Addresses => new Ef5DbSetWrapper<Address>(this);

        IDbSetWrapper<Rota> ITestDbContext.Rotas => new Ef5DbSetWrapper<Rota>(this);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries => new Ef5DbSetWrapper<RotaEntry>(this);

        IDbSetWrapper<OrderUk> ITestDbContext.Orders => new Ef5DbSetWrapper<OrderUk>(this);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems => new Ef5DbSetWrapper<OrderItem>(this);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems => new Ef5DbSetWrapper<PublicBool>(this);

        IDbSetWrapper<PublicByte> ITestDbContext.ByteItems => new Ef5DbSetWrapper<PublicByte>(this);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems => new Ef5DbSetWrapper<PublicShort>(this);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems => new Ef5DbSetWrapper<PublicInt>(this);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems => new Ef5DbSetWrapper<PublicLong>(this);

        IDbSetWrapper<PublicString> ITestDbContext.StringItems => new Ef5DbSetWrapper<PublicString>(this);

        IDbSetWrapper<PublicTitle> ITestDbContext.TitleItems => new Ef5DbSetWrapper<PublicTitle>(this);

        Task ITestDbContext.SaveChanges()
        {
            SaveChanges();

            return Task.CompletedTask;
        }

        #endregion
    }
}