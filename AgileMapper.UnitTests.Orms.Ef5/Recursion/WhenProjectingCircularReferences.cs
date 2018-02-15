namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Recursion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Recursion;
    using Xunit;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<Ef5TestDbContext>,
        IOneToManyRecursionProjectionFailureTest
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