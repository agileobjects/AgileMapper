namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToInts : WhenConvertingToInts<EfCore2TestDbContext>
    {
        public WhenConvertingToInts(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}