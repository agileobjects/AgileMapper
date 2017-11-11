namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToInts : WhenConvertingToInts<EfCore1TestDbContext>
    {
        public WhenConvertingToInts(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}