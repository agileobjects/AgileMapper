namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Infrastructure
{
    using System.Threading.Tasks;
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

        public DbSet<Animal> Animals { get; set;  }

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

        IDbSetWrapper<Animal> ITestDbContext.Animals => new EfCore1DbSetWrapper<Animal>(this);
        
        IDbSetWrapper<Shape> ITestDbContext.Shapes => new EfCore1DbSetWrapper<Shape>(this);

        IDbSetWrapper<Company> ITestDbContext.Companies => new EfCore1DbSetWrapper<Company>(this);

        IDbSetWrapper<Employee> ITestDbContext.Employees => new EfCore1DbSetWrapper<Employee>(this);

        IDbSetWrapper<Category> ITestDbContext.Categories => new EfCore1DbSetWrapper<Category>(this);

        IDbSetWrapper<Product> ITestDbContext.Products => new EfCore1DbSetWrapper<Product>(this);

        IDbSetWrapper<Person> ITestDbContext.Persons => new EfCore1DbSetWrapper<Person>(this);

        IDbSetWrapper<Address> ITestDbContext.Addresses => new EfCore1DbSetWrapper<Address>(this);

        IDbSetWrapper<Rota> ITestDbContext.Rotas => new EfCore1DbSetWrapper<Rota>(this);

        IDbSetWrapper<RotaEntry> ITestDbContext.RotaEntries => new EfCore1DbSetWrapper<RotaEntry>(this);

        IDbSetWrapper<OrderUk> ITestDbContext.Orders => new EfCore1DbSetWrapper<OrderUk>(this);

        IDbSetWrapper<OrderItem> ITestDbContext.OrderItems => new EfCore1DbSetWrapper<OrderItem>(this);

        IDbSetWrapper<PublicBool> ITestDbContext.BoolItems => new EfCore1DbSetWrapper<PublicBool>(this);

        IDbSetWrapper<PublicByte> ITestDbContext.ByteItems => new EfCore1DbSetWrapper<PublicByte>(this);

        IDbSetWrapper<PublicShort> ITestDbContext.ShortItems => new EfCore1DbSetWrapper<PublicShort>(this);

        IDbSetWrapper<PublicInt> ITestDbContext.IntItems => new EfCore1DbSetWrapper<PublicInt>(this);

        IDbSetWrapper<PublicLong> ITestDbContext.LongItems => new EfCore1DbSetWrapper<PublicLong>(this);

        IDbSetWrapper<PublicString> ITestDbContext.StringItems => new EfCore1DbSetWrapper<PublicString>(this);

        IDbSetWrapper<PublicTitle> ITestDbContext.TitleItems => new EfCore1DbSetWrapper<PublicTitle>(this);

        Task ITestDbContext.SaveChanges() => SaveChangesAsync();

        #endregion
    }
}