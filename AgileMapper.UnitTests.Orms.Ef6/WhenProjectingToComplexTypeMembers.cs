namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingToComplexTypeMembers : WhenProjectingToComplexTypeMembers<Ef6TestDbContext>
    {
        public WhenProjectingToComplexTypeMembers(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}