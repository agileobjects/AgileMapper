namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Recursion
{
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
        public void ShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth()
            => DoShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth();
    }
}