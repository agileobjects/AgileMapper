namespace AgileObjects.AgileMapper.UnitTests.Ef6
{
    using Xunit;

    [CollectionDefinition(Name)]
    public class TestContextCollection : ICollectionFixture<TestContext>
    {
        public const string Name = "EF6 collection";
    }
}