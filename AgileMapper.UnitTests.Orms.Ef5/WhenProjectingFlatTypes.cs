namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<Ef5TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}