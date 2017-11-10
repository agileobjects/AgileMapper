namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToBools : WhenConvertingToBools<EfCore2TestDbContext>
    {
        public WhenConvertingToBools(TestContext context)
            : base(context)
        {
        }
    }
}
