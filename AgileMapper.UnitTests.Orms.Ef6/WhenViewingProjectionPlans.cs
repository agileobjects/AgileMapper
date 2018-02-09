namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;

    public class WhenViewingProjectionPlans : WhenViewingProjectionPlans<Ef6TestDbContext>
    {
        public WhenViewingProjectionPlans(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}
