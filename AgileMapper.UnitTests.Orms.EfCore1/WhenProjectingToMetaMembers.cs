namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToMetaMembers : WhenProjectingToMetaMembers<EfCore1TestDbContext>
    {
        public WhenProjectingToMetaMembers(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAHasCollectionMember() => RunShouldProjectToAHasCollectionMember();
    }
}
