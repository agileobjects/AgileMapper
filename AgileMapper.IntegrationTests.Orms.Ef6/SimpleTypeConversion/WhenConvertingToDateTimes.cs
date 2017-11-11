namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using UnitTests.Orms.Infrastructure;
    using UnitTests.Orms.SimpleTypeConversion;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef6TestDbContext>
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef6TestDbContext> context)
            : base(context)
        {
        }
    }
}