namespace AgileObjects.AgileMapper.UnitTests.Ef6
{
    using System.Data.Entity;
    using Effort;
    using TestClasses;

    public class TestDbContext : DbContext
    {
        public TestDbContext()
            : base(DbConnectionFactory.CreateTransient(), true)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<PublicBoolProperty> BoolItems { get; set; }

        public DbSet<PublicShortProperty> ShortItems { get; set; }

        public DbSet<PublicIntProperty> IntItems { get; set; }

        public DbSet<PublicStringProperty> StringItems { get; set; }
    }
}