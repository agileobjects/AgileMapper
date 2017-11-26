namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using Infrastructure;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<EfCore1TestDbContext>
    {
        public WhenProjectingCircularReferences(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }
    }
}