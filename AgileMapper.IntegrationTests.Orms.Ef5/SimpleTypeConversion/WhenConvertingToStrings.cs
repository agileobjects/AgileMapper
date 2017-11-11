namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using UnitTests.Orms.SimpleTypeConversion;

    public class WhenConvertingToStrings : WhenConvertingToStrings<Ef5TestDbContext>
    {
        public WhenConvertingToStrings(Ef5LocalDbTestContext context)
            : base(context)
        {
        }
    }
}