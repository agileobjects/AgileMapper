namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<Ef5TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingToAComplexTypeCollectionMember()
            => RunShouldErrorProjectingToAComplexTypeCollectionMember();
    }
}