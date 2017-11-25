namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;

    public class WhenViewingMappingPlans : WhenViewingMappingPlans<Ef5TestDbContext>
    {
        public WhenViewingMappingPlans(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}
