namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToMetaMembers : WhenProjectingToMetaMembers<Ef6TestDbContext>
    {
        public WhenProjectingToMetaMembers(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAHasCollectionMember() => RunShouldProjectToAHasCollectionMember();
    }
}
