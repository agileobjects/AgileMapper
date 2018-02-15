namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<Ef6TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}
