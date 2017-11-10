namespace AgileObjects.AgileMapper.UnitTests.EfCore2.Infrastructure
{
    using Orms;
    using Orms.Infrastructure;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class TestContextCollection : ICollectionFixture<TestContext>
    {
    }
}