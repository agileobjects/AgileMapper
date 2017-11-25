namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;

    public class WhenViewingMappingPlans : WhenViewingMappingPlans<EfCore2TestDbContext>
    {
        public WhenViewingMappingPlans(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
