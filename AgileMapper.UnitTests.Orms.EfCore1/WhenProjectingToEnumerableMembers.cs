namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<EfCore1TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAComplexTypeCollectionMember()
            => RunShouldProjectToAComplexTypeCollectionMember();
    }
}