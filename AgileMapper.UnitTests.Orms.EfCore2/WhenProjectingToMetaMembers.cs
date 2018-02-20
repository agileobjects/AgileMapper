namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToMetaMembers : WhenProjectingToMetaMembers<EfCore2TestDbContext>
    {
        public WhenProjectingToMetaMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAHasCollectionMember() => RunShouldProjectToAHasCollectionMember();
    }
}
