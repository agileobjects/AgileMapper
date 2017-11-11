namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToStrings : WhenConvertingToStrings<EfCore2TestDbContext>
    {
        public WhenConvertingToStrings(TestContext context)
            : base(context)
        {
        }
    }
}