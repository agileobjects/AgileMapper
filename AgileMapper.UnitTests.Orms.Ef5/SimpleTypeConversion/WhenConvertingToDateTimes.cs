namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef5TestDbContext>
    {
        public WhenConvertingToDateTimes(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}