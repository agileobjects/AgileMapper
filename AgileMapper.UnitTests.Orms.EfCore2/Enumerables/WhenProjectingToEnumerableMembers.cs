namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Enumerables
{
    using Infrastructure;
    using Orms.Enumerables;
    using Xunit;

    public class WhenProjectingToEnumerableMembers :
        WhenProjectingToEnumerableMembers<EfCore2TestDbContext>,
        ICollectionMemberProjectorTest,
        IEnumerableMemberProjectorTest
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore2TestContext context)
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