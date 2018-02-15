namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringEnumMapping : WhenConfiguringEnumMapping<Ef6TestDbContext>
    {
        public WhenConfiguringEnumMapping(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}
