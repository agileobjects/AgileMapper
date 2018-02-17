namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Infrastructure
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

        public DbSet<Animal> Animals { get; set; }

        public DbSet<Shape> Shapes { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Account> Accounts { get; set; }

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

        public DbSet<PublicNullableInt> NullableIntItems { get; set; }

        public DbSet<PublicLong> LongItems { get; set; }

        public DbSet<PublicDecimal> DecimalItems { get; set; }

        public DbSet<PublicDouble> DoubleItems { get; set; }

        public DbSet<PublicDateTime> DateTimeItems { get; set; }

        public DbSet<PublicNullableDateTime> NullableDateTimeItems { get; set; }

        public DbSet<PublicString> StringItems { get; set; }

        public DbSet<PublicStringNames> StringNameItems { get; set; }

        public DbSet<PublicTitle> TitleItems { get; set; }

        public DbSet<PublicNullableTitle> NullableTitleItems { get; set; }

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

            modelBuilder.Entity<Square>();
            modelBuilder.Entity<Circle>();

            modelBuilder.Entity<AccountAddress>()
                .HasKey(aa => new { aa.AccountId, aa.AddressId });

            modelBuilder.Entity<AccountAddress>()
                .HasOne(aa => aa.Account)
                .WithMany(a => a.DeliveryAddresses)
                .HasForeignKey(aa => aa.AccountId);

            base.OnModelCreating(modelBuilder);
        }

        #region ITestDbContext Members

        IDbSetWrapper<Animal> ITestDbContext.Animals => new EfCore2DbSetWrapper<Animal>(this);

        IDbSetWrapper<Shape> ITestDbContext.Shapes => new EfCore2DbSetWrapper<Shape>(this);

        IDbSetWrapper<Company> ITestDbContext.Companies => new EfCore2DbSetWrapper<Company>(this);

        IDbSetWrapper<Employee> ITestDbContext.Employees => new EfCore2DbSetWrapper<Employee>(this);

        IDbSetWrapper<Category> ITestDbContext.Categories => new EfCore2DbSetWrapper<Category>(this);

        IDbSetWrapper<Product> ITestDbContext.Products => new EfCore2DbSetWrapper<Product>(this);

        IDbSetWrapper<Account> ITestDbContext.Accounts => new EfCore2DbSetWrapper<Account>(this);

        IDbSetWrapper<Person> ITestDbContext.Persons => new EfCore2DbSetWrapper<Person>(this);

        IDbSetWrapper<Address> ITestDbContext.Addresses => new EfCore2DbSetWrapper<Address>(this);

        IDbSetWrapper<Rota> ITestDbContext.Rotas => new EfCore2DbSetWrapper<Rota>(this);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries => new EfCore2DbSetWrapper<RotaEntry>(this);

        IDbSetWrapper<OrderUk> ITestDbContext.Orders => new EfCore2DbSetWrapper<OrderUk>(this);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems => new EfCore2DbSetWrapper<OrderItem>(this);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems => new EfCore2DbSetWrapper<PublicBool>(this);

        IDbSetWrapper<PublicByte> ITestDbContext.ByteItems => new EfCore2DbSetWrapper<PublicByte>(this);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems => new EfCore2DbSetWrapper<PublicShort>(this);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems => new EfCore2DbSetWrapper<PublicInt>(this);

        IDbSetWrapper<PublicNullableInt> ITestDbContext.NullableIntItems => new EfCore2DbSetWrapper<PublicNullableInt>(this);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems => new EfCore2DbSetWrapper<PublicLong>(this);

        IDbSetWrapper<PublicDecimal> ITestDbContext.DecimalItems => new EfCore2DbSetWrapper<PublicDecimal>(this);

        IDbSetWrapper<PublicDouble> ITestDbContext.DoubleItems => new EfCore2DbSetWrapper<PublicDouble>(this);

        IDbSetWrapper<PublicDateTime> ITestDbContext.DateTimeItems => new EfCore2DbSetWrapper<PublicDateTime>(this);

        IDbSetWrapper<PublicNullableDateTime> ITestDbContext.NullableDateTimeItems => new EfCore2DbSetWrapper<PublicNullableDateTime>(this);

        IDbSetWrapper<PublicString> ITestDbContext.StringItems => new EfCore2DbSetWrapper<PublicString>(this);

        IDbSetWrapper<PublicTitle> ITestDbContext.TitleItems => new EfCore2DbSetWrapper<PublicTitle>(this);

        IDbSetWrapper<PublicNullableTitle> ITestDbContext.NullableTitleItems => new EfCore2DbSetWrapper<PublicNullableTitle>(this);

        Task ITestDbContext.SaveChanges()
        {
            StringNameItems.RemoveRange(StringNameItems);

            return SaveChangesAsync();
        }

        #endregion
    }
}