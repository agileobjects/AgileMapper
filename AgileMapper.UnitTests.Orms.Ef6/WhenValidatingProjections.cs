namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;

    public class WhenValidatingProjections : WhenValidatingProjections<Ef6TestDbContext>
    {
        public WhenValidatingProjections(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}
