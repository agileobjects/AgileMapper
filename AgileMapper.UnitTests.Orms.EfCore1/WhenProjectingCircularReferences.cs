namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;
    using Recursion;
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
        public void ShouldProjectAOneToManyRelationshipToFirstRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToFirstRecursionDepth();
    }
}