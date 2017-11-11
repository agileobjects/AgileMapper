namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToInts : WhenConvertingToInts<EfCore2TestDbContext>
    {
        public WhenConvertingToInts(InMemoryOrmTestContext context)
            : base(context)
        {
        }
    }
}