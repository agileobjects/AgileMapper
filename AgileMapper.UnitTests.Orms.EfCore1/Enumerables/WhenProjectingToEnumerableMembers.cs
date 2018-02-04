namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Enumerables
{
    using Infrastructure;
    using Orms.Enumerables;
    using Xunit;

    public class WhenProjectingToEnumerableMembers :
        WhenProjectingToEnumerableMembers<EfCore1TestDbContext>,
        ICollectionMemberProjectorTest,
        IEnumerableMemberProjectorTest
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore1TestContext context)
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