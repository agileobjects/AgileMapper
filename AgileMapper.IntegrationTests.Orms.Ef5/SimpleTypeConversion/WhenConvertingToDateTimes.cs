namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using UnitTests.Orms.Infrastructure;
    using UnitTests.Orms.SimpleTypeConversion;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef5TestDbContext>
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef5TestDbContext> context)
            : base(context)
        {
        }
    }
}