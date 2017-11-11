namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<Ef6TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}