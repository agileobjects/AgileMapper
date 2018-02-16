namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingCircularReferences : WhenProjectingCircularReferences<EfCore2TestDbContext>
    {
        public WhenProjectingCircularReferences(InMemoryEfCore2TestContext context)
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