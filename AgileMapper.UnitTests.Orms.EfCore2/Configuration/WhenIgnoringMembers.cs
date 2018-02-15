namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenIgnoringMembers : WhenIgnoringMembers<EfCore2TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
