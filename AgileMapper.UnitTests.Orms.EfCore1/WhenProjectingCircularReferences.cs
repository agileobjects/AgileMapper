namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingCircularReferences : WhenProjectingCircularReferences<EfCore1TestDbContext>
    {
        public WhenProjectingCircularReferences(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAOneToManyRelationshipToDefaultRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToDefaultRecursionDepth();

        [Fact]
        public Task ShouldProjectAOneToManyRelationshipToFirstRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToFirstRecursionDepth();

        [Fact]
        public Task ShouldProjectAOneToManyRelationshipToSecondRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToSecondRecursionDepth();
    }
}