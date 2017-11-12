namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef6TestLocalDbContext>
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef6TestLocalDbContext> context)
            : base(context)
        {
        }
    }
}