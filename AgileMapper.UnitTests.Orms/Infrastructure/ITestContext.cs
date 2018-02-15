namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;

    public interface ITestContext<out TOrmContext> : IDisposable
        where TOrmContext : ITestDbContext, new()
    {
        TOrmContext DbContext { get; }
    }
}
