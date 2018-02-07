namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringConstructorDataSources : WhenConfiguringConstructorDataSources<EfCore2TestDbContext>
    {
        public WhenConfiguringConstructorDataSources(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}