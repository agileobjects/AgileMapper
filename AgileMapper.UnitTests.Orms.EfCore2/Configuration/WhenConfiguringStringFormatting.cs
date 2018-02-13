namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenConfiguringStringFormatting : WhenConfiguringStringFormatting<EfCore2TestDbContext>
    {
        public WhenConfiguringStringFormatting(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
