namespace AgileObjects.AgileMapper.UnitTests.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToBools : WhenConvertingToBools<Ef5TestDbContext>
    {
        public WhenConvertingToBools(TestContext context)
            : base(context)
        {
        }
    }
}
