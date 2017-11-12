namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.Infrastructure
{
    using Orms;
    using Orms.Infrastructure;
    using Xunit;

    [CollectionDefinition(TestConstants.OrmCollectionName)]
    public class Ef6TestLocalDbDefinition : ICollectionFixture<LocalDbTestContext<Ef6TestLocalDbContext>>
    {
    }
}