namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Enumerables;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToEnumerableMembers :
        WhenProjectingToEnumerableMembers<EfCore1TestDbContext>,
        ICollectionMemberProjectorTest
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectToAComplexTypeCollectionMember()
            => RunShouldProjectToAComplexTypeCollectionMember();
    }
}