namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<EfCore1TestDbContext>
    {
        public WhenConvertingToDateTimes(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}