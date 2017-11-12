namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef5TestLocalDbContext>
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }
    }
}