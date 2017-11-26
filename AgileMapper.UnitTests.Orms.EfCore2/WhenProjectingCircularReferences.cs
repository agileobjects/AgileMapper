namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<EfCore2TestDbContext>
    {
        public WhenProjectingCircularReferences(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}