namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.Infrastructure
{
    using Orms;
    using Orms.Infrastructure;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class Ef5TestLocalDbDefinition : ICollectionFixture<LocalDbTestContext<Ef5TestLocalDbContext>>
    {
    }
}