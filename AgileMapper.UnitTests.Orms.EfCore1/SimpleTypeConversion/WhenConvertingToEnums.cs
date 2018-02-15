namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToEnums : WhenConvertingToEnums<EfCore1TestDbContext>
    {
        public WhenConvertingToEnums(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
