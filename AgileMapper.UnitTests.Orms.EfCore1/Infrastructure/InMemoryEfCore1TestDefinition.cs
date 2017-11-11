namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Infrastructure
{
    using Orms;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class InMemoryEfCore1TestDefinition : ICollectionFixture<InMemoryEfCore1TestContext>
    {
    }
}