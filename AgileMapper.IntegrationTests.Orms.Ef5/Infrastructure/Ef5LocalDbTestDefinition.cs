namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef5.Infrastructure
{
    using UnitTests.Orms;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class Ef5LocalDbTestDefinition : ICollectionFixture<Ef5LocalDbTestContext>
    {
    }
}