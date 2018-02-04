namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Recursion
{
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
        public void ShouldProjectAOneToManyRelationshipToZeroethRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToZeroethRecursionDepth();

        [Fact]
        public void ShouldProjectAOneToManyRelationshipToFirstRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToFirstRecursionDepth();

        [Fact]
        public void ShouldProjectAOneToManyRelationshipToSecondRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToSecondRecursionDepth();
    }
}