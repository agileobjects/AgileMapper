namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToEnums : WhenConvertingToEnums<EfCore2TestDbContext>
    {
        public WhenConvertingToEnums(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
