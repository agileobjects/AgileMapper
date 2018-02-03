namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Recursion
{
    using Infrastructure;
    using Orms.Recursion;
    using Xunit;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<EfCore2TestDbContext>,
        IOneToManyRecursionProjectorTest
    {
        public WhenProjectingCircularReferences(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAOneToManyRelationshipToZeroethRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToZeroethRecursionDepth();

        [Fact]
        public void ShouldProjectAOneToManyRelationshipToFirstRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToFirstRecursionDepth();
    }
}