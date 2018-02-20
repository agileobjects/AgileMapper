namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToMetaMembers : WhenProjectingToMetaMembers<Ef5TestDbContext>
    {
        public WhenProjectingToMetaMembers(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingToAHasCollectionMember() => RunShouldErrorProjectingToAHasCollectionMember();
    }
}
