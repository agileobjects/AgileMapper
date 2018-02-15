namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<EfCore2TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAComplexTypeCollectionMember()
            => RunShouldProjectToAComplexTypeCollectionMember();

        [Fact]
        public Task ShouldProjectViaLinkingType() => RunShouldProjectViaLinkingType();
    }
}