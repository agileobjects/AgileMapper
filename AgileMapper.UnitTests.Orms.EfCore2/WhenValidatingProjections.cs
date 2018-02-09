namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;

    public class WhenValidatingProjections : WhenValidatingProjections<EfCore2TestDbContext>
    {
        public WhenValidatingProjections(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}
