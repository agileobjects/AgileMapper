namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToInts : WhenConvertingToInts<Ef5TestDbContext>
    {
        public WhenConvertingToInts(InMemoryOrmTestContext context)
            : base(context)
        {
        }
    }
}