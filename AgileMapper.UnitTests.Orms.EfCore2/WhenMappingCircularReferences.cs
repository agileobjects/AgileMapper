namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using Infrastructure;

    public class WhenMappingCircularReferences :
        WhenMappingCircularReferences<EfCore2TestDbContext>
    {
        public WhenMappingCircularReferences(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }
    }
}