namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;

    public interface ITestContext : IDisposable
    {
        TOrmContext GetDbContext<TOrmContext>()
            where TOrmContext : ITestDbContext, new();
    }
}
