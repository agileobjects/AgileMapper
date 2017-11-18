namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Enumerables;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToEnumerableMembers :
        WhenProjectingToEnumerableMembers<EfCore2TestDbContext>,
        ICollectionMemberProjectorTest
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectToAComplexTypeCollectionMember()
            => RunShouldProjectToAComplexTypeCollectionMember();
    }
}