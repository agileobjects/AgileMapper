namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Infrastructure
{
    using Orms;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class InMemoryEf5TestDefinition : ICollectionFixture<InMemoryEf5TestContext>
    {
    }
}