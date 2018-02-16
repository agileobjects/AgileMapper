namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingCircularReferences : WhenProjectingCircularReferences<Ef5TestDbContext>
    {
        public WhenProjectingCircularReferences(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth()
            => DoShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth();
    }
}