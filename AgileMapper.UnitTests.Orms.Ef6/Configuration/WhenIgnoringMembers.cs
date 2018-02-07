namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenIgnoringMembers : WhenIgnoringMembers<Ef6TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEf6TestContext context)
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
