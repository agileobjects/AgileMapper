namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef6.Infrastructure
{
    using UnitTests.Orms;
    using UnitTests.Orms.Infrastructure;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class Ef6LocalDbTestDefinition : ICollectionFixture<LocalDbTestContext<Ef6TestDbContext>>
    {
    }
}