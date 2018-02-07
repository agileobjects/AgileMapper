namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenIgnoringMembers : WhenIgnoringMembers<Ef5TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldIgnoreAConfiguredMember() => DoShouldIgnoreAConfiguredMember();

        [Fact]
        public Task ShouldIgnoreAConfiguredMemberConditionally() =>
            DoShouldIgnoreAConfiguredMemberConditionally();

        [Fact]
        public Task ShouldIgnorePropertiesByPropertyInfoMatcher()
            => DoShouldIgnorePropertiesByPropertyInfoMatcher();

        [Fact]
        public Task ShouldIgnoreMembersByTypeAndTargetType() => DoShouldIgnoreMembersByTypeAndTargetType();
    }
}
