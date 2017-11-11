namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Infrastructure
{
    using Orms;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class InMemoryEf6TestDefinition : ICollectionFixture<InMemoryEf6TestContext>
    {
    }
}