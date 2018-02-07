namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<EfCore2TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAComplexTypeMemberToAFlatTypeList()
            => DoShouldProjectAComplexTypeMemberToAFlatTypeList();
    }
}
