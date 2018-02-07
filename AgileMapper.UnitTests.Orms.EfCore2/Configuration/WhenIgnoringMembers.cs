namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenIgnoringMembers : WhenIgnoringMembers<EfCore2TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldIgnoreAConfiguredMember() => DoShouldIgnoreAConfiguredMember();

        [Fact]
        public Task ShouldIgnoreAConfiguredMemberConditionally() =>
            DoShouldIgnoreAConfiguredMemberConditionally();
    }
}
