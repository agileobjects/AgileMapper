namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;

    public class WhenValidatingProjections : WhenValidatingProjections<Ef5TestDbContext>
    {
        public WhenValidatingProjections(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}
