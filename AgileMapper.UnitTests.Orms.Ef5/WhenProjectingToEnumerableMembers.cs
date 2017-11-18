namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<Ef5TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}