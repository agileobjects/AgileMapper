namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<Ef5TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAComplexTypeMemberToAFlatTypeList()
            => DoShouldProjectAComplexTypeMemberToAFlatTypeList();
    }
}
