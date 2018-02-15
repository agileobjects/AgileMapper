namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;

    public class WhenViewingProjectionPlans : WhenViewingProjectionPlans<EfCore2TestDbContext>
    {
        public WhenViewingProjectionPlans(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
