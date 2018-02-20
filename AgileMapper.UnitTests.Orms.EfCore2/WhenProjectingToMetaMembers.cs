namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;

    public class WhenProjectingToMetaMembers : WhenProjectingToMetaMembers<EfCore2TestDbContext>
    {
        public WhenProjectingToMetaMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
