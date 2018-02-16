namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingCircularReferences : WhenProjectingCircularReferences<Ef6TestDbContext>
    {
        public WhenProjectingCircularReferences(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth()
            => DoShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth();
    }
}