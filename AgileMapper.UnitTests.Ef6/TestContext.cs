namespace AgileObjects.AgileMapper.UnitTests.Ef6
{
    using System;

    public class TestContext : IDisposable
    {
        public TestContext()
        {
            DbContext = new TestDbContext();
        }

        public TestDbContext DbContext { get; }

        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}
