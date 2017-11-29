namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;
    using Recursion;
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
        public void ShouldErrorProjectingAOneToManyRelationshipToFirstRecursionDepth()
            => DoShouldErrorProjectingAOneToManyRelationshipToFirstRecursionDepth();
    }
}