namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef6TestDbContext>
    {
        public WhenConvertingToDateTimes(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}