namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Infrastructure
{
    using Orms;
    using Orms.Infrastructure;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class InMemoryOrmTestDefinition : ICollectionFixture<InMemoryOrmTestContext>
    {
    }
}