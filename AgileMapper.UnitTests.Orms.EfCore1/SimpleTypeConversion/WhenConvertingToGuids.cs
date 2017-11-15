namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToGuids : WhenConvertingToGuids<EfCore1TestDbContext>
    {
        public WhenConvertingToGuids(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}