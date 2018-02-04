namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<EfCore1TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
