namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToBools : WhenConvertingToBools<Ef5TestDbContext>
    {
        public WhenConvertingToBools(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}
