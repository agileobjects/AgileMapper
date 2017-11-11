namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToStrings : WhenConvertingToStrings<EfCore1TestDbContext>
    {
        public WhenConvertingToStrings(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}