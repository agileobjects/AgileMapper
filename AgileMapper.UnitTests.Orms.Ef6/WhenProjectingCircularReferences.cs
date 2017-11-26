namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using Infrastructure;

    public class WhenProjectingCircularReferences :
        WhenProjectingCircularReferences<Ef6TestDbContext>
    {
        public WhenProjectingCircularReferences(InMemoryEf6TestContext context)
            : base(context)
        {
        }
    }
}