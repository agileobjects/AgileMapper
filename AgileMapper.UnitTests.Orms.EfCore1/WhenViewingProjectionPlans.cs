namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;

    public class WhenViewingProjectionPlans : WhenViewingProjectionPlans<EfCore1TestDbContext>
    {
        public WhenViewingProjectionPlans(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
