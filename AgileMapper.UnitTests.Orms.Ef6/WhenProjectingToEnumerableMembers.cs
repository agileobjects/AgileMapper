namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<Ef6TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAComplexTypeCollectionMember()
            => RunShouldProjectToAComplexTypeCollectionMember();
    }
}