namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringEnumMapping : WhenConfiguringEnumMapping<EfCore2TestDbContext>
    {
        public WhenConfiguringEnumMapping(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
