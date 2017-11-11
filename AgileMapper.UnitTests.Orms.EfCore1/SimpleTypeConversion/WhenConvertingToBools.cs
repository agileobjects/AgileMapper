namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToBools : WhenConvertingToBools<EfCore1TestDbContext>
    {
        public WhenConvertingToBools(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
