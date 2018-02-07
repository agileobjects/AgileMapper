namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<EfCore1TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAComplexTypeMemberToAFlatTypeList()
            => DoShouldProjectAComplexTypeMemberToAFlatTypeList();
    }
}
