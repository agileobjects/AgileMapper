namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingToComplexTypeMembers : WhenProjectingToComplexTypeMembers<EfCore1TestDbContext>
    {
        public WhenProjectingToComplexTypeMembers(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}