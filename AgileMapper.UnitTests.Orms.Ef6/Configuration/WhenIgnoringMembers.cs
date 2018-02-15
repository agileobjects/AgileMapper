namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using Infrastructure;
    using Orms.Configuration;

    public class WhenIgnoringMembers : WhenIgnoringMembers<Ef6TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}
