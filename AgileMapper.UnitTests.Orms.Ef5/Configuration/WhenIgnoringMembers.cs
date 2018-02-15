namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenIgnoringMembers : WhenIgnoringMembers<Ef5TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}
