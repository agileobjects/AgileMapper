namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingToComplexTypeMembers : WhenProjectingToComplexTypeMembers<EfCore2TestDbContext>
    {
        public WhenProjectingToComplexTypeMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}