namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<EfCore1TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}