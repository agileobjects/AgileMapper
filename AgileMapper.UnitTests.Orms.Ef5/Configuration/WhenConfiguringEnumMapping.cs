namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringEnumMapping : WhenConfiguringEnumMapping<Ef5TestDbContext>
    {
        public WhenConfiguringEnumMapping(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}
