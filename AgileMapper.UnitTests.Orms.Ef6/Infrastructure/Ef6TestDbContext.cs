namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Threading.Tasks;
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

        public DbSet<Animal> Animals { get; set; }

        public DbSet<Shape> Shapes { get; set; }

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

        public DbSet<PublicDecimal> DecimalItems { get; set; }

        public DbSet<PublicDouble> DoubleItems { get; set; }

        public DbSet<PublicDateTime> DateTimeItems { get; set; }

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

        IDbSetWrapper<Animal> ITestDbContext.Animals => new Ef6DbSetWrapper<Animal>(this);

        IDbSetWrapper<Shape> ITestDbContext.Shapes => new Ef6DbSetWrapper<Shape>(this);

        IDbSetWrapper<Company> ITestDbContext.Companies => new Ef6DbSetWrapper<Company>(this);

        IDbSetWrapper<Employee> ITestDbContext.Employees => new Ef6DbSetWrapper<Employee>(this);

        IDbSetWrapper<Category> ITestDbContext.Categories => new Ef6DbSetWrapper<Category>(this);

        IDbSetWrapper<Product> ITestDbContext.Products => new Ef6DbSetWrapper<Product>(this);

        IDbSetWrapper<Person> ITestDbContext.Persons => new Ef6DbSetWrapper<Person>(this);

        IDbSetWrapper<Address> ITestDbContext.Addresses => new Ef6DbSetWrapper<Address>(this);

        IDbSetWrapper<Rota> ITestDbContext.Rotas => new Ef6DbSetWrapper<Rota>(this);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries => new Ef6DbSetWrapper<RotaEntry>(this);

        IDbSetWrapper<OrderUk> ITestDbContext.Orders => new Ef6DbSetWrapper<OrderUk>(this);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems => new Ef6DbSetWrapper<OrderItem>(this);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems => new Ef6DbSetWrapper<PublicBool>(this);

        IDbSetWrapper<PublicByte> ITestDbContext.ByteItems => new Ef6DbSetWrapper<PublicByte>(this);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems => new Ef6DbSetWrapper<PublicShort>(this);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems => new Ef6DbSetWrapper<PublicInt>(this);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems => new Ef6DbSetWrapper<PublicLong>(this);

        IDbSetWrapper<PublicDecimal> ITestDbContext.DecimalItems => new Ef6DbSetWrapper<PublicDecimal>(this);

        IDbSetWrapper<PublicDouble> ITestDbContext.DoubleItems => new Ef6DbSetWrapper<PublicDouble>(this);

        IDbSetWrapper<PublicDateTime> ITestDbContext.DateTimeItems => new Ef6DbSetWrapper<PublicDateTime>(this);
        
        IDbSetWrapper<PublicString> ITestDbContext.StringItems => new Ef6DbSetWrapper<PublicString>(this);

        IDbSetWrapper<PublicTitle> ITestDbContext.TitleItems => new Ef6DbSetWrapper<PublicTitle>(this);

        Task ITestDbContext.SaveChanges() => SaveChangesAsync();

        #endregion
    }
}