namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    public interface ITestLocalDbContext : ITestDbContext
    {
        void CreateDatabase();

        void DeleteDatabase();
    }
}