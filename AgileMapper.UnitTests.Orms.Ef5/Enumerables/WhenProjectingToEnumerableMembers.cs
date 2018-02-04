namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Enumerables
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Enumerables;
    using Xunit;

    public class WhenProjectingToEnumerableMembers :
        WhenProjectingToEnumerableMembers<Ef5TestDbContext>,
        ICollectionMemberProjectionFailureTest,
        IEnumerableMemberProjectorTest
    {
        public WhenProjectingToEnumerableMembers(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingToAComplexTypeCollectionMember()
            => RunShouldErrorProjectingToAComplexTypeCollectionMember();

        [Fact]
        public Task ShouldProjectToAComplexTypeEnumerableMember()
            => RunShouldProjectToAComplexTypeEnumerableMember();
    }
}