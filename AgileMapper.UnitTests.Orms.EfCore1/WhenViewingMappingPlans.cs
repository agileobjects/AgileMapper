namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;

    public class WhenViewingMappingPlans : WhenViewingMappingPlans<EfCore1TestDbContext>
    {
        public WhenViewingMappingPlans(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
