namespace AgileObjects.AgileMapper.UnitTests.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToInts : WhenConvertingToInts<Ef5TestDbContext>
    {
        public WhenConvertingToInts(TestContext context)
            : base(context)
        {
        }
    }
}