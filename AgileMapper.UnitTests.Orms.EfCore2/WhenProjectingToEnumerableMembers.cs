namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;
    using Orms;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<EfCore2TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}