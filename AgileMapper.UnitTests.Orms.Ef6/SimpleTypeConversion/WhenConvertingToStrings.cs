namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToStrings : WhenConvertingToStrings<Ef6TestDbContext>
    {
        public WhenConvertingToStrings(InMemoryOrmTestContext context)
            : base(context)
        {
        }
    }
}