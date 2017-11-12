namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToStrings : WhenConvertingToStrings<Ef5TestLocalDbContext>
    {
        public WhenConvertingToStrings(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }
    }
}