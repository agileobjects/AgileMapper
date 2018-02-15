namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenIgnoringMembers : WhenIgnoringMembers<EfCore1TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
