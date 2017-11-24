namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Enumerables;
    using Infrastructure;
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
        public void ShouldProjectToAComplexTypeCollectionMember()
            => RunShouldProjectToAComplexTypeCollectionMember();

        [Fact]
        public void ShouldProjectToAComplexTypeEnumerableMember()
            => RunShouldProjectToAComplexTypeEnumerableMember();
    }
}