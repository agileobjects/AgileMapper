namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using UnitTests.Orms.Infrastructure;
    using UnitTests.Orms.SimpleTypeConversion;

    public class WhenConvertingToStrings : WhenConvertingToStrings<Ef5TestLocalDbContext>
    {
        public WhenConvertingToStrings(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }
    }
}