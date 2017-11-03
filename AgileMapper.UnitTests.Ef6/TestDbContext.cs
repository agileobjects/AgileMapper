namespace AgileObjects.AgileMapper.UnitTests.Ef6
{
    using System.Data.Entity;
    using Effort;

    public class TestDbContext : DbContext
    {
        public TestDbContext()
            : base(DbConnectionFactory.CreateTransient(), true)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}