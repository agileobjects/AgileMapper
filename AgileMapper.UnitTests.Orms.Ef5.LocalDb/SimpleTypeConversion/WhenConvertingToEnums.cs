namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToEnums : WhenConvertingToEnums<Ef5TestLocalDbContext>
    {
        public WhenConvertingToEnums(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }
    }
}
