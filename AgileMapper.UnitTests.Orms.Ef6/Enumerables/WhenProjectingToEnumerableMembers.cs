namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Enumerables
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Enumerables;
    using Xunit;

    public class WhenProjectingToEnumerableMembers :
        WhenProjectingToEnumerableMembers<Ef6TestDbContext>,
        ICollectionMemberProjectorTest,
        IEnumerableMemberProjectorTest
    {
        public WhenProjectingToEnumerableMembers(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAComplexTypeCollectionMember()
            => RunShouldProjectToAComplexTypeCollectionMember();

        [Fact]
        public Task ShouldProjectToAComplexTypeEnumerableMember()
            => RunShouldProjectToAComplexTypeEnumerableMember();
    }
}