namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToStrings : WhenConvertingToStrings<Ef5TestDbContext>
    {
        public WhenConvertingToStrings(TestContext context)
            : base(context)
        {
        }
    }
}