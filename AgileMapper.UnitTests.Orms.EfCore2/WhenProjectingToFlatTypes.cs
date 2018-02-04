namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<EfCore2TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
