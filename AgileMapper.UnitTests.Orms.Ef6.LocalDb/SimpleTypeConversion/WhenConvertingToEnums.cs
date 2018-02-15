namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToEnums : WhenConvertingToEnums<Ef6TestLocalDbContext>
    {
        public WhenConvertingToEnums(LocalDbTestContext<Ef6TestLocalDbContext> context)
            : base(context)
        {
        }
    }
}
