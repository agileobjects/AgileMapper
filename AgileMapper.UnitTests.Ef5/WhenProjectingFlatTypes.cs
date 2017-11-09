namespace AgileObjects.AgileMapper.UnitTests.Ef5
{
    using Infrastructure;
    using Orms;
    using Orms.Infrastructure;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<Ef5TestDbContext>
    {
        public WhenProjectingFlatTypes(TestContext context)
            : base(context)
        {
        }
    }
}