namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<Ef5TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}
