namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToBools : WhenConvertingToBools<EfCore2TestDbContext>
    {
        public WhenConvertingToBools(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
