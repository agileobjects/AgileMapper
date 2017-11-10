namespace AgileObjects.AgileMapper.UnitTests.EfCore2
{
    using Infrastructure;
    using Orms;
    using Orms.Infrastructure;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<EfCore2TestDbContext>
    {
        public WhenProjectingFlatTypes(TestContext context)
            : base(context)
        {
        }
    }
}