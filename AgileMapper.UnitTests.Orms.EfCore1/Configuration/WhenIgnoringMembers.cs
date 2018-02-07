namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenIgnoringMembers : WhenIgnoringMembers<EfCore1TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEfCore1TestContext context)
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
