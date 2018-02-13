namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenMappingToNull : WhenMappingToNull<EfCore2TestDbContext>
    {
        public WhenMappingToNull(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
