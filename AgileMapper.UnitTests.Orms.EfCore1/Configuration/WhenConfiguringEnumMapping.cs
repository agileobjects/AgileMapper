namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringEnumMapping : WhenConfiguringEnumMapping<EfCore1TestDbContext>
    {
        public WhenConfiguringEnumMapping(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
