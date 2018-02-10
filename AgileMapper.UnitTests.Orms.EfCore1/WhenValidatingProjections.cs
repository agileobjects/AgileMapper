namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;

    public class WhenValidatingProjections : WhenValidatingProjections<EfCore1TestDbContext>
    {
        public WhenValidatingProjections(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}
