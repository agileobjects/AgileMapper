namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Recursion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Recursion;
    using Xunit;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<EfCore1TestDbContext>,
        IOneToManyRecursionProjectorTest
    {
        public WhenProjectingCircularReferences(InMemoryEfCore1TestContext context)
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