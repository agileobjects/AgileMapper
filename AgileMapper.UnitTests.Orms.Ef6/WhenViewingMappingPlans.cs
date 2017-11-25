namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;

    public class WhenViewingMappingPlans : WhenViewingMappingPlans<Ef6TestDbContext>
    {
        public WhenViewingMappingPlans(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}
