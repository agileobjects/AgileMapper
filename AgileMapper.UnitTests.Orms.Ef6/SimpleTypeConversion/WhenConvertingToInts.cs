namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToInts : WhenConvertingToInts<Ef6TestDbContext>
    {
        public WhenConvertingToInts(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}