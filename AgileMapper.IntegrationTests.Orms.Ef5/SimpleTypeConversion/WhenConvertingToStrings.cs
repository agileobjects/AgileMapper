namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using UnitTests.Orms.Infrastructure;
    using UnitTests.Orms.SimpleTypeConversion;

    public class WhenConvertingToStrings : WhenConvertingToStrings<Ef5TestDbContext>
    {
        public WhenConvertingToStrings(LocalDbTestContext<Ef5TestDbContext> context)
            : base(context)
        {
        }
    }
}