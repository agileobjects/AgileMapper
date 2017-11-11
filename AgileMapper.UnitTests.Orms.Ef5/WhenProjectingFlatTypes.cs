namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;
    using Orms;
    using Orms.Infrastructure;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<Ef5TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryOrmTestContext context)
            : base(context)
        {
        }
    }
}