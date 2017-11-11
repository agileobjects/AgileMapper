namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;
    using Orms;
    using Orms.Infrastructure;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<EfCore2TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryOrmTestContext context)
            : base(context)
        {
        }
    }
}