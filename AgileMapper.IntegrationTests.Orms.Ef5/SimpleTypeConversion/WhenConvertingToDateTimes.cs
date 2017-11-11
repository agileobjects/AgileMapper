namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using UnitTests.Orms.Infrastructure;
    using UnitTests.Orms.SimpleTypeConversion;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef5TestLocalDbContext>
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }
    }
}