namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Infrastructure;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<Ef5TestDbContext>
    {
        public WhenProjectingCircularReferences(InMemoryEf5TestContext context)
            : base(context)
        {
        }
    }
}