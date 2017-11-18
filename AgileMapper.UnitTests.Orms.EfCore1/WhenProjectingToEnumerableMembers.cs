namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<EfCore1TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}