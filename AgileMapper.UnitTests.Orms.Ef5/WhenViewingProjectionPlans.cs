namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;

    public class WhenViewingProjectionPlans : WhenViewingProjectionPlans<Ef5TestDbContext>
    {
        public WhenViewingProjectionPlans(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}
