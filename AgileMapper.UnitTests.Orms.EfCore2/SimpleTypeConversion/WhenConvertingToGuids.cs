namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToGuids : WhenConvertingToGuids<EfCore2TestDbContext>
    {
        public WhenConvertingToGuids(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}