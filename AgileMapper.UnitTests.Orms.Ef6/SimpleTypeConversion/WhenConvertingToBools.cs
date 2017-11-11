namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToBools : WhenConvertingToBools<Ef6TestDbContext>
    {
        public WhenConvertingToBools(InMemoryOrmTestContext context)
            : base(context)
        {
        }
    }
}
