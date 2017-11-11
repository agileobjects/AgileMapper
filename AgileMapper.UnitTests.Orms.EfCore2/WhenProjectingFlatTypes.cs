namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<EfCore2TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}