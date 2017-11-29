namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;
    using Recursion;
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
        public void ShouldProjectAOneToManyRelationshipToFirstRecursionDepth()
            => DoShouldProjectAOneToManyRelationshipToFirstRecursionDepth();
    }
}