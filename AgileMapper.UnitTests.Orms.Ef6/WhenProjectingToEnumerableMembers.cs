namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<Ef6TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}