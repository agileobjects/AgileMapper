namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;
    using Recursion;
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
        public void ShouldErrorProjectingAOneToManyRelationshipToFirstRecursionDepth()
            => DoShouldErrorProjectingAOneToManyRelationshipToFirstRecursionDepth();
    }
}