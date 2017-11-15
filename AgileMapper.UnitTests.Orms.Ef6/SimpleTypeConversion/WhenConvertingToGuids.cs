namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToGuids : WhenConvertingToGuids<Ef6TestDbContext>
    {
        public WhenConvertingToGuids(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}