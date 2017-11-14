namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingToComplexTypeMembers : WhenProjectingToComplexTypeMembers<Ef5TestDbContext>
    {
        public WhenProjectingToComplexTypeMembers(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}