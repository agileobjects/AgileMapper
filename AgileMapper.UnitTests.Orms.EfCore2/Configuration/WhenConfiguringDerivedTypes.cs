namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringDerivedTypes : WhenConfiguringDerivedTypes<EfCore2TestDbContext>
    {
        public WhenConfiguringDerivedTypes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
