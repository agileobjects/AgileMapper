namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Infrastructure
{
    using Orms;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class InMemoryEfCore2TestDefinition : ICollectionFixture<InMemoryEfCore2TestContext>
    {
    }
}