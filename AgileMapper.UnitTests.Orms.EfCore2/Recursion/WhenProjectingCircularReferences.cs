namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Recursion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Recursion;
    using Xunit;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<EfCore2TestDbContext>,
        IOneToManyRecursionProjectorTest
    {
        public WhenProjectingCircularReferences(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAOneToManyRelationshipToZeroethRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToZeroethRecursionDepth();

        [Fact]
        public Task ShouldProjectAOneToManyRelationshipToFirstRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToFirstRecursionDepth();

        [Fact]
        public Task ShouldProjectAOneToManyRelationshipToSecondRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToSecondRecursionDepth();

        [Fact]
        public Task ShouldProjectAOneToManyRelationshipToDefaultRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToDefaultRecursionDepth();
    }
}