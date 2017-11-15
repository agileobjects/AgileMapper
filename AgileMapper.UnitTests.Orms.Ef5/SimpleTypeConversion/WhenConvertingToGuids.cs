namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToGuids : WhenConvertingToGuids<Ef5TestDbContext>
    {
        public WhenConvertingToGuids(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}