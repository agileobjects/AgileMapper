namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Infrastructure
{
    using Orms;
    using Orms.Infrastructure;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class TestContextCollection : ICollectionFixture<TestContext>
    {
    }
}