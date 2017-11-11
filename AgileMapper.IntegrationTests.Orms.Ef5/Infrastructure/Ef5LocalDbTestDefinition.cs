namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.Infrastructure
{
    using UnitTests.Orms;
    using UnitTests.Orms.Infrastructure;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class Ef5LocalDbTestDefinition : ICollectionFixture<LocalDbTestContext<Ef5TestDbContext>>
    {
    }
}