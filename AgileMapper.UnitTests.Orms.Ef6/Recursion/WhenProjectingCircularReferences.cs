namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Recursion
{
    using Infrastructure;
    using Orms.Recursion;
    using Xunit;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<Ef6TestDbContext>,
        IOneToManyRecursionProjectionFailureTest
    {
        public WhenProjectingCircularReferences(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth()
            => DoShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth();
    }
}