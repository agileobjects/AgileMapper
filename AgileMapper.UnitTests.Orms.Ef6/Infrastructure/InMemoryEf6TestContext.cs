namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Infrastructure
{
    using Orms.Infrastructure;

    public class InMemoryEf6TestContext : InMemoryOrmTestContext<Ef6TestDbContext>
    {
        public InMemoryEf6TestContext()
        {
            // ReSharper disable once UnusedVariable
            // Touch SqlFunctions to load System.Data.Entity into the AppDomain:
            //var functionsType = typeof(SqlFunctions);
        }
    }
}